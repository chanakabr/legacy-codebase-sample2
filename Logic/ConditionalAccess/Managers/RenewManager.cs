using APILogic.ConditionalAccess.Managers;
using APILogic.ConditionalAccess.Modules;
using APILogic.ConditionalAccess.Response;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.ConditionalAccess.Modules;
using Core.Pricing;
using Core.Users;
using DAL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TVinciShared;

namespace Core.ConditionalAccess
{
    public class RenewManager
    {
        #region Consts

        private const string ILLEGAL_CONTENT_ID = "Illegal content ID";
        private const string MAX_USAGE_MODULE = "mumlc";
        protected const string ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION = "PROCESS_RENEW_SUBSCRIPTION\\{0}";
        private const int PENDING_THRESHOLD_DAYS = 180;

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Static Methods

        public static bool Renew(BaseConditionalAccess cas, int groupId, string siteguid, long purchaseId, string billingGuid, long nextEndDate, ref bool shouldUpdateTaskStatus)
        {
            // log request
            string logString = string.Format("Purchase request: siteguid {0}, purchaseId {1}, billingGuid {2}, endDateLong {3}", siteguid, purchaseId, billingGuid, nextEndDate);

            log.DebugFormat("Starting renewal process. data: {0}", logString);

            string customData = string.Empty;
            long householdId = 0;

            string userIp = "1.1.1.1";

            // validate purchaseId
            if (purchaseId <= 0 || string.IsNullOrEmpty(billingGuid))
            {
                // Illegal purchase ID  
                log.ErrorFormat("Illegal purchaseId or billingGuid. data: {0}", logString);
                return true;
            }

            #region Get subscription purchase

            // get subscription purchase 
            DataRow subscriptionPurchaseRow = DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseForRenewal(groupId, purchaseId, billingGuid);

            // validate subscription received
            if (subscriptionPurchaseRow == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("problem getting the subscription purchase. Purchase ID: {0}, data: {1}", purchaseId, logString);
                shouldUpdateTaskStatus = false;
                return false;
            }

            #endregion

            // get product ID
            long productId = ODBCWrapper.Utils.ExtractInteger(subscriptionPurchaseRow, "SUBSCRIPTION_CODE"); // AKA subscription ID/CODE
            string couponCode = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "COUPON_CODE");

            ResponseStatus userValidStatus = ResponseStatus.OK;
            userValidStatus = Utils.ValidateUser(groupId, siteguid, ref householdId, true);

            /******* Check if this is a renew via INAPP PURCHASE **********/
            PaymentDetails pd = null;
            ApiObjects.Response.Status statusVerifications = Billing.Module.GetPaymentGatewayVerificationStatus(groupId, billingGuid, ref pd);            
            bool ignoreUnifiedBillingCycle = statusVerifications.Code != (int)eResponseStatus.OK || pd == null || pd.PaymentGatewayId == 0;

            if (statusVerifications.Code != (int)eResponseStatus.PaymentGatewayNotValid && !APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, siteguid, RolePermissions.RENEW_SUBSCRIPTION))
            {
                // mark this subscription in special status 
                if (!ConditionalAccessDAL.UpdateMPPRenewalSubscriptionStatus(new List<int>() { (int)purchaseId }, (int)SubscriptionPurchaseStatus.Suspended))
                {
                    log.ErrorFormat("Failed to suspend purchase id  entitlements for payment gateway: UpdateMPPRenewalSubscriptionStatus fail in DB purchaseId={0}, householdId={1}", purchaseId, householdId);
                }
                log.ErrorFormat("domain is not permitted to renew process . details : {0}", logString);
                return true;
            }
                        
            if (userValidStatus == ResponseStatus.UserSuspended)
            {
                userValidStatus = ResponseStatus.OK;
            }

            // get end date
            DateTime endDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionPurchaseRow, "END_DATE");

            long endDateUnix =TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate);

            // validate renewal did not already happened
            if (Math.Abs(endDateUnix - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling renew task. Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), logString);
                return true;
            }

            string previousPurchaseCurrencyCode = string.Empty;
            string previousPurchaseCountryCode = string.Empty;
            string previousPurchaseCountryName = string.Empty;

            #region Dummy

            try
            {
                customData = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "CUSTOMDATA"); // AKA subscription ID/CODE

                if (userValidStatus == ResponseStatus.OK && !string.IsNullOrEmpty(customData))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(customData);
                    XmlNode theRequest = doc.FirstChild;
                    // previousPurchaseCurrencyCode, previousPurchaseCountryCode and previousPurchaseCountryName will be used later for getting the correct priceCodeData 
                    previousPurchaseCurrencyCode = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                    previousPurchaseCountryName = XmlUtils.GetSafeValue(BaseConditionalAccess.COUNTRY_CODE, ref theRequest);
                    previousPurchaseCountryCode = Utils.GetCountryCodeByCountryName(groupId, previousPurchaseCountryName);
                    bool isDummy = XmlUtils.IsNodeExists(ref theRequest, BaseConditionalAccess.DUMMY);
                    if (isDummy)
                    {
                        return HandleDummySubsciptionRenewal(cas, groupId, siteguid, purchaseId, billingGuid, logString, householdId, userIp, productId, theRequest,
                            endDate, customData);
                    }
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Renew: Error while getting data from xml, data: {0}, error: {1}", logString, exc);
                return false;
            }

            #endregion

            // validate user ID
            string purchaseSiteguid = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "SITE_USER_GUID");
            if (purchaseSiteguid != siteguid)
            {
                // siteguid not equal to purchase siteguid
                log.ErrorFormat("siteguid {0} not equal to purchase siteguid {1}. data: {2}", siteguid, purchaseSiteguid, logString);
                return true;
            }

            log.DebugFormat("subscription purchase found and validated. data: {0}", logString);

            // validate user object               
            bool shouldSwitchToMasterUser = false;

            // check if we need to set shouldSwitchToMasterUser = true so we will update subscription details to master user instead of user where needed

            #region shouldSwitchToMasterUser

            if (userValidStatus == ResponseStatus.UserDoesNotExist)
            {
                shouldSwitchToMasterUser = true;
                householdId = ODBCWrapper.Utils.GetLongSafeVal(subscriptionPurchaseRow, "DOMAIN_ID");
                string masterSiteGuid = string.Empty;
                if (householdId > 0)
                {
                    Domain domain = Utils.GetDomainInfo((int)householdId, groupId);
                    if (domain != null && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
                    {
                        masterSiteGuid = domain.m_masterGUIDs.First().ToString();
                    }
                }

                if (string.IsNullOrEmpty(masterSiteGuid))
                {
                    // could not find a master user to replace the deleted user                   
                    log.ErrorFormat("User validation failed: UserDoesNotExist and no MasterUser to replace in renew, data: {0}", logString);
                    return true;
                }
                else
                {
                    log.WarnFormat("SiteGuid: {0} does not exist, changing renew SiteGuid value to MasterSiteGuid: {1}", siteguid, masterSiteGuid);
                    siteguid = masterSiteGuid;
                }
            }

           

            // check if response OK only if we know response is not UserDoesNotExist, shouldSwitchToMasterUser is set to false by default
            if (!shouldSwitchToMasterUser && userValidStatus != ResponseStatus.OK)
            {
                // user validation failed
                ApiObjects.Response.Status status = Utils.SetResponseStatus(userValidStatus);
                log.ErrorFormat("User validation failed: {0}, data: {1}", status.Message, logString);
                return true;
            }

            #endregion

            // validate household
            if (householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }

            // get transaction details
            //--------------------------------------------
            DataRow renewDetailsRow = DAL.ConditionalAccessDAL.Get_RenewDetails(groupId, purchaseId, billingGuid);

            if (renewDetailsRow == null)
            {
                // transaction details weren't found
                log.ErrorFormat("Transaction details weren't found. Product ID: {0}, billing GUID: {1}, data: {2}", productId, billingGuid, logString);
                return false;
            }

            log.DebugFormat("Renew details received. data: {0}", logString);

            #region Get Subscription data

            Subscription subscription = null;
            try
            {
                subscription = Core.Pricing.Module.GetSubscriptionData(groupId, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. data: {0}", logString), ex);
                return false;
            }


            // validate subscription
            if (subscription == null)
            {
                // subscription wasn't found
                log.Error(string.Format("subscription wasn't found. productId {0}, data: {1}", productId, logString));
                return false;
            }

            log.DebugFormat("Subscription data received. data: {0}", logString);

            #endregion

            if (subscription.Type == SubscriptionType.AddOn && subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
            {
                ApiObjects.Response.Status status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    // change is recurring to false and call event handle
                    // renew subscription failed!
                    bool handleNonRenew = HandleRenewSubscriptionFailed(cas, groupId,
                        siteguid, purchaseId, logString, productId, subscription, householdId, 0, "AddOn with no BaseSubscription valid",
                        billingGuid, endDateUnix);

                    log.ErrorFormat("failed renew subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}", subscription.m_SubscriptionCode, status.Code, status.Message);
                    return true;
                }
            }

            // get payment number
            int paymentNumber = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "PAYMENT_NUMBER");

            // get number of payments
            int numOfPayments = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "number_of_payments");

            // get total number of payments
            int totalNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "total_number_of_payments");

            // check if purchased with preview module
            bool isPurchasedWithPreviewModule;
            isPurchasedWithPreviewModule = ApiDAL.Get_IsPurchasedWithPreviewModuleByBillingGuid(groupId, billingGuid, (int)purchaseId);

            paymentNumber = Utils.CalcPaymentNumber(numOfPayments, paymentNumber, isPurchasedWithPreviewModule);
            if (numOfPayments > 0 && paymentNumber > numOfPayments)
            {
                // Subscription ended
                log.ErrorFormat("Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}", numOfPayments, paymentNumber, numOfPayments);
                cas.WriteToUserLog(siteguid, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                    productId, numOfPayments, paymentNumber, numOfPayments, billingGuid));
                return true;
            }

            // calculate payment number
            paymentNumber++;

            // get compensation data
            Compensation compensation = ConditionalAccessDAL.GetSubscriptionCompensationByPurchaseId(purchaseId);

            // get MPP
            int recPeriods = 0;
            bool isMPPRecurringInfinitely = false;
            int maxVLCOfSelectedUsageModule = 0;
            double price = 0;
            string currency = "n/a";
            UnifiedBillingCycle unifiedBillingCycle = null;

            if (!cas.GetMultiSubscriptionUsageModule(siteguid, userIp, (int)purchaseId, paymentNumber, totalNumOfPayments, numOfPayments, isPurchasedWithPreviewModule,
                    ref price, ref customData, ref currency, ref recPeriods, ref isMPPRecurringInfinitely, ref maxVLCOfSelectedUsageModule,
                    ref couponCode, subscription, ref unifiedBillingCycle, compensation, previousPurchaseCountryName, previousPurchaseCountryCode, previousPurchaseCurrencyCode,
                    endDate, groupId, householdId, ignoreUnifiedBillingCycle))
            {
                // "Error while trying to get Price plan
                log.Error("Error while trying to get Price plan to renew");
                return false;
            }

            if (!string.IsNullOrEmpty(couponCode))
                ignoreUnifiedBillingCycle = true;

            long unifiedProcessId = 0;
            if (!ignoreUnifiedBillingCycle && unifiedBillingCycle != null) //should be part of unified cycle 
            {
                unifiedProcessId = UpdateMPPRenewalProcessId(groupId, purchaseId, billingGuid, householdId, unifiedBillingCycle, pd != null ? pd.PaymentGatewayId : 0);                
            }

            // call billing process renewal
            TransactResult transactionResponse = null;
            try
            {
                transactionResponse = Core.Billing.Module.ProcessRenewal(groupId, siteguid, householdId, price, currency,
                                            customData, (int)productId, subscription.m_ProductCode, paymentNumber, numOfPayments, billingGuid, subscription.m_GracePeriodMinutes);
            }
            catch (Exception ex)
            {
                // error while trying to process renew in billing
                log.Error("Error while calling the billing process renewal", ex);
                return false;
            }

            log.DebugFormat("Renew transaction returned from billing. data: {0}", logString);

            if (transactionResponse == null ||
                transactionResponse.Status == null)
            {
                // PG returned error
                log.Error("Received error from Billing");
                return false;
            }

            if (transactionResponse.Status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Received error from Billing.ProcessRenewal code:{0}, msg:{1}", transactionResponse.Status.Code, transactionResponse.Status.Message);

                if (transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotSetForHousehold ||
                    transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotExist ||
                    transactionResponse.Status.Code == (int)eResponseStatus.PaymentGatewayNotExist)
                {
                    // renew subscription failed! pass 0 as failReasonCode since we don't get it on the transactionResponse
                    return HandleRenewSubscriptionFailed(cas, groupId,
                        siteguid, purchaseId, logString, productId, subscription, householdId, 0, transactionResponse.Status.Message,
                        billingGuid, endDateUnix);
                }
                else if (transactionResponse.Status.Code == (int)eResponseStatus.PaymentGatewaySuspended)
                {
                    if (!ConditionalAccessDAL.UpdateMPPRenewalSubscriptionStatus(new List<int>() { (int)purchaseId }, (int)SubscriptionPurchaseStatus.Suspended))
                    {
                        log.ErrorFormat("Failed to suspend purchase id  entitlements for payment gateway: UpdateMPPRenewalSubscriptionStatus fail in DB purchaseId={0}, householdId={1}", purchaseId, householdId);
                    }
                    return true;// don't retry
                }
                else
                {
                    return false;
                }
            }

            bool res = false;
            switch (transactionResponse.State)
            {
                case eTransactionState.OK:
                    {
                        res = HandleRenewSubscriptionSuccess(cas, groupId, siteguid, purchaseId, billingGuid, logString, productId, ref endDate, householdId, price, currency, paymentNumber,
                            totalNumOfPayments, subscription, customData, maxVLCOfSelectedUsageModule, transactionResponse, unifiedBillingCycle, unifiedProcessId);
                        if (res)
                        {
                            string invalidationKey = LayeredCacheKeys.GetRenewInvalidationKey(householdId);
                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                            {
                                log.ErrorFormat("Failed to set invalidation key on Renew key = {0}", invalidationKey);
                            }

                            // update compensation use
                            if (compensation != null)
                            {
                                if (!ConditionalAccessDAL.UpdateSubscriptionCompensationUse(compensation.Id, transactionResponse.TransactionID, compensation.Renewals + 1))
                                {
                                    log.ErrorFormat("Failed to update subscription compensation use. compensationId = {0}, billingTransactionId = {1}, renewalNumber = {2}",
                                        compensation.Id, transactionResponse.TransactionID, compensation.Renewals + 1);
                                }
                            }
                        }
                    }
                    break;
                case eTransactionState.Pending:
                    {
                        // renew subscription pending!
                        res = HandleRenewSubscriptionPending(cas, groupId,
                            siteguid, purchaseId, billingGuid, logString, productId, endDate, householdId, shouldSwitchToMasterUser, price, currency);
                    }
                    break;
                case eTransactionState.Failed:
                    {
                        // renew subscription failed!
                        res = HandleRenewSubscriptionFailed(cas, groupId,
                            siteguid, purchaseId, logString, productId, subscription, householdId, transactionResponse.FailReasonCode, null,
                            billingGuid, endDateUnix);

                    }
                    break;
                default:
                    {
                        log.Error("Transaction state is unknown");
                    }
                    break;
            }
            return res;
        }

        // get right process id from DB and update this row in DB 
        private static long UpdateMPPRenewalProcessId(int groupId, long purchaseId, string billingGuid, long householdId, UnifiedBillingCycle unifiedBillingCycle, int paymentGatewayId)
        {
            long processId = 0;
            try
            {
                if (paymentGatewayId == 0)
                {
                    PaymentGateway pg = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, householdId, billingGuid);
                    if (pg != null && pg.ID > 0)
                    {
                        paymentGatewayId = pg.ID;
                    }
                }

                if (paymentGatewayId > 0)
                { 
                    DataTable dt = DAL.ConditionalAccessDAL.GetUnifiedProcessIdByHouseholdPaymentGateway(groupId, paymentGatewayId, householdId);

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        processId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");                        
                    }

                    if (processId == 0) // need to create new process id 
                    {
                        processId = ConditionalAccessDAL.InsertUnifiedProcess(groupId, paymentGatewayId, ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate), 
                                                                                householdId, (int)ProcessUnifiedState.Renew);                        
                    }

                    if (processId > 0)
                    {
                        // update subscription Purchase
                        DAL.ConditionalAccessDAL.UpdateMPPRenewalProcessId(new List<int>() { (int)purchaseId }, processId);
                    }
                }
            }
            catch (Exception ex)
            {
                processId = 0;
                log.ErrorFormat("fail update process id for specific renew groupId = {0}, purchaseId = {1}, billingGuid={2}, householdId = {3}, ex = {4}", groupId, purchaseId, billingGuid, householdId, ex);
            }
            return processId;
        }

        protected internal static bool HandleRenewSubscriptionFailed(BaseConditionalAccess cas, int groupId,
            string siteguid, long purchaseId, string logString, long productId,
            Subscription subscription, long domainId, int failReasonCode, string billingSettingError = null, string billingGuid = null,
            long endDateUnix = 0)
        {
            log.DebugFormat("Transaction renew failed. data: {0}", logString);

            // grant entitlement
            SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(groupId)
            {
                purchaseId = (int)purchaseId,
                siteGuid = siteguid,
                productId = subscription.m_SubscriptionCode,
                status = SubscriptionPurchaseStatus.Fail
            };

            bool success = subscriptionPurchase.Update();

            if (!success)
            {
                log.Error("Error while trying to cancel subscription");
                return false;
            }
            else
            {
                log.Debug("Subscription was canceled");
            }

            cas.WriteToUserLog(siteguid, string.Format("Transaction renew failed. Product ID: {0}, purchase ID: {1}",
                 productId,                           // {0}
                 purchaseId));                        // {1}

            // PS message 
            var dicData = new Dictionary<string, object>()
                                        {
                                            {"SiteGUID", siteguid},
                                            {"DomainID", domainId},
                                            {"FailReasonCode", failReasonCode},
                                            {"PurchaseID", purchaseId},
                                            {"SubscriptionCode", subscription.m_SubscriptionCode}
                                        };

            if (failReasonCode == 0)
            {
                dicData.Add("BillingSettingError", billingSettingError);
            }

            cas.EnqueueEventRecord(NotifiedAction.FailedSubscriptionRenewal, dicData);

            // Enqueue event for subscription ends:
            EnqueueSubscriptionEndsMessage(groupId, siteguid, purchaseId, endDateUnix);

            return true;
        }

        internal static void EnqueueSubscriptionEndsMessage(int groupId, string siteguid, long purchaseId, long endDateUnix)
        {
            RenewTransactionsQueue queue = new RenewTransactionsQueue();

            DateTime endDate = TVinciShared.DateUtils.UnixTimeStampToDateTime(endDateUnix);

            RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseId, string.Empty,
                            endDateUnix, endDate, eSubscriptionRenewRequestType.SubscriptionEnds);

            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of subscription ends event {0}", data);
            }
        }

        protected internal static bool HandleRenewSubscriptionPending(BaseConditionalAccess cas, int groupId,
            string siteguid, long purchaseId, string billingGuid, string logString, long productId, DateTime endDate,
            long householdId, bool shouldSwitchToMasterUser, double price, string currency)
        {
            log.DebugFormat("Transaction renew pending. data: {0}", logString);

            // get billing gateway
            PaymentGateway paymentGatewayResponse = null;
            try
            {
                // check if to update siteGuid in subscription_purchases to masterSiteGuid and set renewal to masterSiteGuid
                if (shouldSwitchToMasterUser)
                {
                    ConditionalAccessDAL.Update_SubscriptionPurchaseRenewalSiteGuid(groupId, billingGuid, (int)purchaseId, siteguid, "CA_CONNECTION_STRING");
                }

                paymentGatewayResponse = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, householdId, billingGuid);

                DateTime nextRenewalDate = DateTime.UtcNow.AddMinutes(60); // default

                if (paymentGatewayResponse == null)
                {
                    // error getting PG
                    log.Error("Error while trying to get the PG");
                }
                else if (paymentGatewayResponse.RenewalIntervalMinutes > 0)
                {
                    nextRenewalDate = DateTime.UtcNow.AddMinutes(paymentGatewayResponse.RenewalIntervalMinutes);
                }

                // enqueue renew transaction
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), nextRenewalDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                    return false;
                }
                else
                {
                    PurchaseManager.SendRenewalReminder(data, householdId);
                    log.DebugFormat("New task created (upon renew pending response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }

            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get the PG", ex);
                return false;
            }

            log.DebugFormat("pending renew returned. subID: {0}, price: {1}, currency: {2}, userID: {3}",
                productId, // {0}
                price,     // {1}
                currency,  // {2}
                siteguid); // {3}
            cas.WriteToUserLog(siteguid, string.Format("pending renew returned. Product ID: {0}, price: {1}, currency: {2}, purchase ID: {3}",
                  productId,                           // {0}
                  price,                               // {1}
                  currency,                            // {2}
                  purchaseId));                        // {3}

            return true;
        }

        protected static bool HandleRenewSubscriptionSuccess(BaseConditionalAccess cas, int groupId,
            string siteguid, long purchaseId, string billingGuid, string logString, long productId, ref DateTime endDate, long householdId,
            double price, string currency, int paymentNumber, int totalNumOfPayments, Subscription subscription, string customData, int maxVLCOfSelectedUsageModule,
            TransactResult transactionResponse, UnifiedBillingCycle unifiedBillingCycle, long unifiedProcessId)
        {
            // renew subscription success!
            log.DebugFormat("Transaction renew success. data: {0}", logString);

            // get billing gateway
            PaymentGateway paymentGateway;

            try
            {
                paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, householdId, billingGuid);
                if (paymentGateway == null)
                {
                    // error getting PG
                    log.Error("Transaction occurred! Error while trying to get the PG");
                }
            }
            catch (Exception ex)
            {
                log.Error("Transaction occurred! Error while trying to get the PG", ex);
                return true;
            }

            // update end-date
            if (transactionResponse.EndDateSeconds > 0)
            {
                // end-date returned: EndDate = PG_End_Date + Configured_PG_Start_Renew_Time
                endDate = DateUtils.UnixTimeStampToDateTime(transactionResponse.EndDateSeconds);
                log.DebugFormat("New end-date was updated according to PG. EndDate={0}", endDate);
            }
            else
            {
                // update unified billing cycle for domian with next end date
                if (unifiedProcessId > 0)
                {
                    long? groupBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
                    if (groupBillingCycle.HasValue && (int)groupBillingCycle.Value == maxVLCOfSelectedUsageModule)
                    {
                        if (unifiedBillingCycle == null)
                        {
                            unifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle((int)householdId, groupBillingCycle.Value);
                        }

                        if (unifiedBillingCycle != null)
                        {
                            if (unifiedBillingCycle.endDate < ODBCWrapper.Utils.DateTimeToUnixTimestampMilliseconds(endDate))
                            {
                                // update unified billing by endDate or paymentGatewatId                  
                                bool setResult = UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(householdId, groupBillingCycle.Value, ODBCWrapper.Utils.DateTimeToUnixTimestampMilliseconds(endDate));
                            }
                            else
                            {
                                endDate = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate);
                            }

                            log.DebugFormat("New end-date was updated according to UnifiedBillingCycle. EndDate={0}", endDate);
                        }
                    }
                }
                else
                {
                    // end wasn't retuned - get next end date from MPP
                    endDate = Utils.GetEndDateTime(endDate, maxVLCOfSelectedUsageModule);
                    log.DebugFormat("New end-date was updated according to MPP. EndDate={0}", endDate);
                }
            }


            // update MPP renew data
            try
            {
                ConditionalAccessDAL.Update_MPPRenewalData(purchaseId, true, endDate, 0, "CA_CONNECTION_STRING", siteguid);
                cas.WriteToUserLog(siteguid, string.Format("Successfully renewed. Product ID: {0}, price: {1}, currency: {2}, purchase ID: {3}, Billing Transition ID: {4}",
                    productId,                           // {0}
                    price,                               // {1}
                    currency,                            // {2}
                    purchaseId,                          // {3}
                    transactionResponse.TransactionID)); // {4}
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to update MPP renew data", ex);
                return true;
            }

            // update billing_transactions subscriptions_purchased reference  
            if (transactionResponse.TransactionID > 0)
            {
                ApiDAL.Update_PurchaseIDInBillingTransactions(transactionResponse.TransactionID, purchaseId);
            }
            else
            {
                log.Error("Error while trying update billing_transactions subscriptions_purchased reference");
            }

            if (unifiedBillingCycle == null)
            {
                // enqueue renew transaction
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                DateTime nextRenewalDate = endDate.AddMinutes(-5);

                if (paymentGateway != null)
                {
                    nextRenewalDate = endDate.AddMinutes(paymentGateway.RenewalStartMinutes);
                }
                RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), nextRenewalDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                    return true;
                }
                else
                {
                    PurchaseManager.SendRenewalReminder(data, householdId);
                    log.DebugFormat("New task created (upon renew success response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }
            }

            // PS message 
            var dicData = new Dictionary<string, object>()
                                        {
                                            {"BillingTransactionID", transactionResponse.TransactionID},
                                            {"SiteGUID", siteguid},
                                            {"PaymentNumber", paymentNumber},
                                            {"TotalPaymentsNumber", totalNumOfPayments},
                                            {"CustomData", customData},
                                            {"Price", price},
                                            {"PurchaseID", purchaseId},
                                            {"SubscriptionCode", subscription.m_SubscriptionCode}
                                        };

            cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, dicData);

            log.DebugFormat("Successfully renewed. productId: {0}, price: {1}, currency: {2}, userID: {3}, billingTransactionId: {4}",
                productId,                          // {0}
                price,                              // {1}
                currency,                           // {2}
                siteguid,                           // {3}
                transactionResponse.TransactionID); // {4}

            return true;
        }

        internal static bool GiftCardReminder(BaseConditionalAccess cas, int groupId, string userId, long purchaseId, string billingGuid, long nextEndDate)
        {
            bool success = false;

            // log request
            string logString = string.Format("GiftCardReminder request: userId {0}, purchaseId {1}, billingGuid {2}, endDateLong {3}", userId, purchaseId, billingGuid, nextEndDate);

            log.DebugFormat("Starting GiftCardReminder process. data: {0}", logString);

            long householdId = 0;

            // validate purchaseId
            if (purchaseId <= 0 || string.IsNullOrEmpty(billingGuid))
            {
                // Illegal purchase ID  
                log.ErrorFormat("Illegal purchaseId or billingGuid. data: {0}", logString);
                return true;
            }

            #region Get subscription purchase

            // get subscription purchase data
            List<string> subscriptionPurchaseColumns = new List<string>() { };

            DataRow subscriptionPurchaseRow =
                DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseForReminder(groupId, purchaseId);

            // validate subscription received
            if (subscriptionPurchaseRow == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("problem getting the subscription purchase. Purchase ID: {0}, data: {1}", purchaseId, logString);
                return false;
            }

            #endregion

            // get product ID
            long productId = ODBCWrapper.Utils.ExtractInteger(subscriptionPurchaseRow, "SUBSCRIPTION_CODE"); // AKA subscription ID/CODE

            Domain domain;
            User user;
            var userValidStatus = Utils.ValidateUserAndDomain(groupId, userId, ref householdId, out domain, out user);

            // validate household
            if ((userValidStatus == null || userValidStatus.Code != (int)eResponseStatus.OK) &&
                householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }

            // get end date
            DateTime endDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionPurchaseRow, "END_DATE");

            // validate renewal did not already happened
            if (Math.Abs(TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate) - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling GiftCardReminder task." +
                    "Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), logString);
                return true;
            }

            PaymentDetails paymentDetail = null;

            // call billing get payment details
            try
            {
                List<PaymentDetails> paymentDetails = Core.Billing.Module.GetPaymentDetails(groupId, new List<string>() { billingGuid });
                paymentDetail = paymentDetails != null ? paymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                // error while trying to process renew in billing
                log.Error("Error while calling the billing GetPaymentDetails", ex);
                return false;
            }

            // we will send a reminder mail only if we don't have a payment method set for this user/household/billing guid
            if (paymentDetail != null)
            {
                log.DebugFormat("GiftCardReminder - user {0} set payment method for billing guid {1}, not sending reminder email", userId, billingGuid);
            }
            else
            {
                Subscription subscription = null;
                try
                {
                    subscription = Core.Pricing.Module.GetSubscriptionData(groupId, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error while trying to fetch subscription data. data: {0}", logString), ex);
                    return false;
                }

                string itemName = subscription.m_sObjectVirtualName;
                try
                {
                    GiftCardReminderMailRequest giftCardRequest =
                        GetGiftCardReminderTemplate(groupId, user, itemName, endDate);

                    if (giftCardRequest != null)
                    {
                        log.DebugFormat("params for gift card reminder mail ws_cas .m_sSubject={0}, houseHoldUser.m_sSiteGUID={1}, purchaseRequest.m_sTemplateName={2}",
                            giftCardRequest.m_sSubject, userId, giftCardRequest.m_sTemplateName);

                        if (!string.IsNullOrEmpty(giftCardRequest.m_sTemplateName))
                        {
                            success = Core.Api.Module.SendMailTemplate(groupId, giftCardRequest);

                            log.DebugFormat("Gift card reminder, WS_API.SendMailTemplate result: {0}. For: siteGuid={1}, itemName={2}, purchaseId={3}",
                                success, user, itemName, purchaseId);
                        }
                        else
                        {
                            log.ErrorFormat("Gift card reminder email for site guid {0} anda purchase id {1} failed because template name is empty", userId, purchaseId);
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Gift card reminder email for site guid {0} anda purchase id {1} failed because gift card mail request wasn't created properly",
                            userId, purchaseId);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Send gift card reminder mail - " + String.Concat("Exception. ", userId, " | ", ex.Message, " | ", ex.StackTrace), ex);
                }
            }

            success = true;

            return success;
        }

        #endregion

        public static GiftCardReminderMailRequest GetGiftCardReminderTemplate(int groupId, User user, string itemName, DateTime endDate)
        {
            GiftCardReminderMailRequest reminderTemplate = new GiftCardReminderMailRequest();

            reminderTemplate.itemName = itemName;

            // user info
            reminderTemplate.m_sSenderTo = user.m_oBasicData.m_sEmail;
            reminderTemplate.m_sLastName = user.m_oBasicData.m_sLastName;
            reminderTemplate.m_sFirstName = user.m_oBasicData.m_sFirstName;

            // days left and end date
            reminderTemplate.daysLeft = ((int)Math.Round((endDate - DateTime.UtcNow).TotalDays)).ToString();
            string dateEmailFormat = Billing.Utils.GetDateEmailFormat(groupId);
            reminderTemplate.endDate = endDate.ToString(dateEmailFormat);

            // get template data from groups parameters table
            List<string> columns = new List<string>() { "GIFT_CARD_REMINDER_MAIL_SUBJECT", "GIFT_CARD_REMINDER_MAIL_TEMPLATE_NAME", "MAIL_FROM_NAME", "MAIL_FROM_ADD" };
            var groupsParameters = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("groups_parameters", "GROUP_ID", groupId.ToString(), columns, "BILLING_CONNECTION_STRING");

            if (groupsParameters != null)
            {
                reminderTemplate.m_sTemplateName = ODBCWrapper.Utils.ExtractString(groupsParameters, "GIFT_CARD_REMINDER_MAIL_TEMPLATE_NAME");
                reminderTemplate.m_sSubject = ODBCWrapper.Utils.ExtractString(groupsParameters, "GIFT_CARD_REMINDER_MAIL_SUBJECT");
                reminderTemplate.m_sSenderFrom = ODBCWrapper.Utils.ExtractString(groupsParameters, "MAIL_FROM_ADD");
                reminderTemplate.m_sSenderName = ODBCWrapper.Utils.ExtractString(groupsParameters, "MAIL_FROM_NAME");
            }

            return reminderTemplate;
        }

        protected static bool HandleRenewGrantedSubscription(BaseConditionalAccess cas, int groupId, string siteguid, long purchaseId, string billingGuid,
            long productId, ref DateTime endDate, long householdId, double price, string currency, int paymentNumber, int totalNumOfPayments, Subscription subscription,
            string customData, int maxVLCOfSelectedUsageModule, long billingTransitionId)
        {

            // end wasn't retuned - get next end date from MPP
            endDate = Utils.GetEndDateTime(endDate, maxVLCOfSelectedUsageModule);
            log.DebugFormat("New end-date was updated according to MPP. EndDate={0}", endDate);

            // update MPP renew data
            try
            {
                ConditionalAccessDAL.Update_MPPRenewalData(purchaseId, true, endDate, 0, "CA_CONNECTION_STRING", siteguid);
                cas.WriteToUserLog(siteguid, string.Format("Successfully renewed. Product ID: {0}, price: {1}, currency: {2}, purchase ID: {3}, Billing Transition ID: {4}",
                    productId,                           // {0}
                    price,                               // {1}
                    currency,                            // {2}
                    purchaseId,                          // {3}
                    billingTransitionId));               // {4}
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to update MPP renew data", ex);
                return true;
            }

            string invalidationKey = LayeredCacheKeys.GetRenewInvalidationKey(householdId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key on Renew key = {0}", invalidationKey);
            }

            // update billing_transactions subscriptions_purchased reference  
            if (billingTransitionId > 0 && !ApiDAL.Update_PurchaseIDInBillingTransactions(billingTransitionId, purchaseId))
            {
                log.Error("Error while trying update billing_transactions subscriptions_purchased reference");
            }

            // enqueue renew transaction
            RenewTransactionsQueue queue = new RenewTransactionsQueue();
            DateTime nextRenewalDate = endDate.AddMinutes(0);

            RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), nextRenewalDate);
            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                return true;
            }
            else
            {
                PurchaseManager.SendRenewalReminder(data, householdId);
                log.DebugFormat("New task created (upon renew success response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
            }

            // PS message 
            if (billingTransitionId > 0)
            {
                var dicData = new Dictionary<string, object>()
                                        {
                                            {"BillingTransactionID", billingTransitionId},
                                            {"SiteGUID", siteguid},
                                            {"PaymentNumber", paymentNumber},
                                            {"TotalPaymentsNumber", totalNumOfPayments},
                                            {"CustomData", customData},
                                            {"Price", price},
                                            {"PurchaseID", purchaseId},
                                            {"SubscriptionCode", subscription.m_SubscriptionCode}
                                        };

                cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, dicData);
            }

            log.DebugFormat("Successfully renewed. productId: {0}, price: {1}, currency: {2}, userID: {3}, billingTransactionId: {4}",
                productId,                          // {0}
                price,                              // {1}
                currency,                           // {2}
                siteguid,                           // {3}
                billingTransitionId);               // {4}

            return true;
        }

        protected internal static bool HandleDummySubsciptionRenewal(BaseConditionalAccess cas, int groupId, string userId, long purchaseId, string billingGuid,
            string logString, long householdId, string userIp, long productId, XmlNode theRequest, DateTime endDate, string customData)
        {
            bool saveHistory = XmlUtils.IsNodeExists(ref theRequest, BaseConditionalAccess.HISTORY);
            string udid = XmlUtils.GetSafeValue(BaseConditionalAccess.DEVICE_NAME, ref theRequest);
            int newRecurringNumber = 0;
            int oldRecurringNumber = 0;
            int numOfPayments = 0;

            if (!int.TryParse(XmlUtils.GetSafeValue(BaseConditionalAccess.RECURRING_NUMBER, ref theRequest), out oldRecurringNumber))
            {
                // Subscription ended
                log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse recurringNumber,  data: {0}", logString);
                cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                    productId, numOfPayments, oldRecurringNumber, numOfPayments, billingGuid));
                return false;
            }

            if (!int.TryParse(XmlUtils.GetSafeParValue("//p", "o", ref theRequest), out numOfPayments))
            {
                // Subscription ended
                log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse //p o,  data: {0}", logString);
                cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                    productId, numOfPayments, oldRecurringNumber, numOfPayments, billingGuid));
                return false;

            }
            newRecurringNumber = Utils.CalcPaymentNumber(numOfPayments, oldRecurringNumber, false);
            if (numOfPayments > 0 && newRecurringNumber > numOfPayments)
            {
                // Subscription ended
                log.ErrorFormat("Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}", numOfPayments, newRecurringNumber, numOfPayments);
                cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                    productId, numOfPayments, newRecurringNumber, numOfPayments, billingGuid));
                return true;
            }

            // calculate payment (recurring) number
            newRecurringNumber++;

            string price = XmlUtils.GetSafeValue(BaseConditionalAccess.PRICE, ref theRequest);
            string currency = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
            string mumlc = XmlUtils.GetSafeValue(MAX_USAGE_MODULE, ref theRequest);

            Subscription subscription = null;
            string pricingUsername = string.Empty, pricingPassword = string.Empty;
            Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref pricingUsername, ref pricingPassword);
            Subscription[] subscriptions = Utils.GetSubscriptionsDataWithCaching(new List<long>(1) { productId }, groupId);
            if (subscriptions != null && subscriptions.Length > 0)
            {
                subscription = subscriptions[0];
            }
            else
            {
                return true;
            }

            if (newRecurringNumber != oldRecurringNumber)
            {
                customData = customData.Replace(string.Format("<recurringnumber>{0}</recurringnumber>", oldRecurringNumber), string.Format("<recurringnumber>{0}</recurringnumber>", newRecurringNumber));
            }

            long billingTransactionID = 0;

            if (saveHistory)
            {
                // purchase
                BillingResponse billingResponse = new BillingResponse();
                billingResponse.m_oStatus = BillingResponseStatus.UnKnown;

                billingResponse = cas.HandleCCChargeUser(userId, double.Parse(price), currency, userIp, customData,
                       newRecurringNumber, numOfPayments, string.Empty, string.Empty, string.Empty, true, false);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success || !long.TryParse(billingResponse.m_sRecieptCode, out billingTransactionID))
                {
                    return false;
                }

            }

            return HandleRenewGrantedSubscription(cas, groupId, userId, purchaseId, billingGuid, productId, ref endDate, householdId,
               double.Parse(price), currency, newRecurringNumber, numOfPayments, subscription, theRequest.InnerXml, int.Parse(mumlc), billingTransactionID);

        }

        public static bool RenewUnifiedTransaction(BaseConditionalAccess cas, int groupId, long householdId, long nextEndDate,
            long processId, ref List<long> purchasesIds)
        {
            // log request
            string logString = string.Format("RenewUnifiedTransaction: householdId {0}, processId {1}, endDateLong {2}", householdId, processId, nextEndDate);

            log.DebugFormat("Starting renewal one transaction process. data: {0}", logString);

            string customData = string.Empty;

            string userIp = "1.1.1.1";

            // get unified billing cycle for this household 
            long? groupBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
            if (!groupBillingCycle.HasValue)
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no groupBillingCycle define in CB for groupId={0}", groupId);
                return true;
            }

            #region validate domain
            Domain domain = null;
            // validate household
            if (householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }
            
            ApiObjects.Response.Status domainStatus = Utils.ValidateDomain(groupId, (int)householdId, out domain);
            if (domain == null || domainStatus == null || domainStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("RenewUnifiedTransaction ValidateDomain householdId = {0} status.Code = {1} ", householdId, domainStatus.Code);
                return true;
            }

            // check if this user permitted to renew 
            if (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count() > 0)
            {
                if (!APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, domain.m_masterGUIDs[0].ToString(), RolePermissions.RENEW_SUBSCRIPTION))
                {
                    // mark this subscription in special status 
                   // get all purchases ids by process ids 
                    if (!ConditionalAccessDAL.UpdateMPPRenewalSubscriptionStatus(null, (int)SubscriptionPurchaseStatus.Suspended, groupId, householdId, processId))                      
                    {
                        log.ErrorFormat("Failed to suspend process id  entitlements : UpdateMPPRenewalSubscriptionStatus fail in DB processId={0}, householdId={1}", processId, householdId);
                    }

                    log.ErrorFormat("domain is not permitted to renew process . details : {0}", logString);
                    return true;
                }
            }            

            #endregion

            // Get Process Details 
            int paymentgatewayId = 0;
            ProcessUnifiedState processState = ProcessUnifiedState.Renew;
            DateTime? processEndDate = null;
            if (UpdateProcessDetailsForRenewal(processId, ref paymentgatewayId, ref processState, ref processEndDate))
            {
                // validate that this is the right message                              
                if (Math.Abs(ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(processEndDate.Value) - nextEndDate) > 60)
                {
                    // subscription purchase wasn't found
                    log.ErrorFormat("GetProcessDetails if end date not equal - canceling unified renew task. processId: {0}, nextEndDate: {1}, data: {2}",
                        processId, nextEndDate, logString);
                    bool changeUnifiedStatus = ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null);
                    return true;
                }
            }
            else 
            {
                log.DebugFormat("No data return from DB for processId {0}", processId);
                return true;
            }

            UnifiedBillingCycle unifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle((int)householdId, groupBillingCycle.Value);
            if (unifiedBillingCycle == null)
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no unifiedBillingCycle define in CB for householdid ={0} ", householdId);
                return true;
            }

            #region Get subscriptions purchase and renew details

            // get subscription purchase 
            DataTable subscriptionPurchaseDt = DAL.ConditionalAccessDAL.Set_SubscriptionPurchaseUnifiedForRenewal(groupId, householdId, processId);

            // validate subscription received
            if (subscriptionPurchaseDt == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("problem getting the subscription purchase. householdId : {0}, data: {1}", householdId, logString);
                return false; // retry
            }

            List<RenewSubscriptionDetails> renewSubscriptioDetails = Utils.BuildSubscriptionPurchaseDetails(subscriptionPurchaseDt, groupId, cas);
            if (renewSubscriptioDetails == null || renewSubscriptioDetails.Count() == 0)// all resDetails were addon with none relevant base
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no addon subscriptions for householdid = {0} and paymentgatewayid = { 1 }", householdId, paymentgatewayId);
                return true;
            }

            purchasesIds = renewSubscriptioDetails.Select(s => s.PurchaseId).ToList();

            #endregion

            #region Get Subscriptions data

            List<Subscription> subscriptions = null;
            try
            {
                subscriptions = Utils.GetSubscriptionsDataWithCaching(renewSubscriptioDetails.Select(x => x.ProductId).ToList(), groupId).ToList();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. data: {0}", logString), ex);
                return false;
            }

            // validate subscription
            if (subscriptions == null || subscriptions.Count() == 0)
            {
                // subscription wasn't found
                log.Error(string.Format("subscription wasn't found. productIds {0}, data: {1}", string.Join(",", renewSubscriptioDetails.Select(x => x.ProductId).ToList()), logString));
                return false;
            }

            log.DebugFormat("Subscription data received. data: {0}", logString);

            #endregion

            log.DebugFormat("subscription purchases found and validated. data: {0}", logString);

            #region - this section is not relevant for NOW
            /*
            // validate user object               
            bool shouldSwitchToMasterUser = false;

            // check if we need to set shouldSwitchToMasterUser = true so we will update subscription details to master user instead of user where needed
            #region shouldSwitchToMasterUser

            //if (userValidStatus == ResponseStatus.UserDoesNotExist)
            //{
            //    shouldSwitchToMasterUser = true;
            //    householdId = ODBCWrapper.Utils.GetLongSafeVal(subscriptionPurchaseRow, "DOMAIN_ID");
            //    string masterSiteGuid = string.Empty;
            //    if (householdId > 0)
            //    {
            //        Domain domain = Utils.GetDomainInfo((int)householdId, groupId);
            //        if (domain != null && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
            //        {
            //            masterSiteGuid = domain.m_masterGUIDs.First().ToString();
            //        }
            //    }

            //    if (string.IsNullOrEmpty(masterSiteGuid))
            //    {
            //        // could not find a master user to replace the deleted user                   
            //        log.ErrorFormat("User validation failed: UserDoesNotExist and no MasterUser to replace in renew, data: {0}", logString);
            //        return true;
            //    }
            //    else
            //    {
            //        log.WarnFormat("SiteGuid: {0} does not exist, changing renew SiteGuid value to MasterSiteGuid: {1}", siteguid, masterSiteGuid);
            //        siteguid = masterSiteGuid;
            //    }
            //}

            // check if response OK only if we know response is not UserDoesNotExist, shouldSwitchToMasterUser is set to false by default
            //if (!shouldSwitchToMasterUser && userValidStatus != ResponseStatus.OK)
            //{
            //    // user validation failed
            //    ApiObjects.Response.Status status = Utils.SetResponseStatus(userValidStatus);
            //    log.ErrorFormat("User validation failed: {0}, data: {1}", status.Message, logString);
            //    return true;
            //}

            #endregion
            
            */
            #endregion

            #region check addon subscriptions
            List<string> removeSubscriptionCodes = new List<string>();
            List<Subscription> baseSubscriptions = subscriptions.Where(x => x.Type == SubscriptionType.Base).ToList();
            foreach (Subscription subscription in (subscriptions.Where(x => x.Type == SubscriptionType.AddOn).ToList()))
            {
                if (subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
                {
                    ApiObjects.Response.Status status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        // check mabye this add on have base subscription in this unified billing cycle 
                        bool canPurchaseAddOn = false;
                        // get all setsIds for this addon 
                        List<long> addOnSetIds = subscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();
                        // check if one of the subscription are base in this unified cycle 
                        foreach (Subscription baseSubscription in baseSubscriptions)
                        {
                            List<long> baseSetIds = baseSubscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();
                            if (baseSetIds.Where(x => addOnSetIds.Contains(x)).Count() > 0)
                            {
                                canPurchaseAddOn = true;
                            }
                        }
                        if (!canPurchaseAddOn)
                        {
                            // change is recurring to false and call event handle- this renew subscription failed!                        

                            RenewSubscriptionDetails rsDetail = renewSubscriptioDetails.Where(x => x.ProductId == subscription.m_SubscriptionCode).FirstOrDefault();
                            if (HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, 
                                rsDetail, 0, "AddOn with no BaseSubscription valid", 
                                string.Empty, nextEndDate))
                            {
                                // save all SubscriptionCode to remove from subscription list 
                                removeSubscriptionCodes.Add(subscription.m_SubscriptionCode);
                                // remove this renewDetails (its an AddOn)
                                renewSubscriptioDetails.Remove(rsDetail);
                            }

                            log.ErrorFormat("failed renew subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}", subscription.m_SubscriptionCode, status.Code, status.Message);
                        }
                    }
                }
            }

            if (renewSubscriptioDetails == null || renewSubscriptioDetails.Count() == 0)// all resDetails were addon with none relevant base
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no addon subscriptions for householdid = {0} and paymentgatewayid = { 1 }", householdId, paymentgatewayId);
                return true;
            }
            if (removeSubscriptionCodes.Count() > 0) // remove all not relevant subscriptions
            {
                subscriptions.RemoveAll(x => removeSubscriptionCodes.Contains(x.m_SubscriptionCode));
            }
            #endregion

            Utils.GetMultiSubscriptionUsageModule(renewSubscriptioDetails, userIp, subscriptions, cas, ref unifiedBillingCycle, (int)householdId, groupId);

            // call billing process renewal
            TransactResult transactionResponse = null;
            PaymentGateway paymentGateway = null;

            // call to each group of subscription that have the same : paymentmethode id + currency                
            Dictionary<string, List<RenewSubscriptionDetails>> renewUnifiedDict = renewSubscriptioDetails.GroupBy(r => string.Format("{0}_{1}", r.PaymentMethodId, r.Currency)).ToDictionary(g => g.Key, g => g.ToList());

            // save dictionary for the resulte from adapter - handle it after 
            List<string> successTransactions = new List<string>();
            List<string> pendingTransactions = new List<string>();

            foreach (KeyValuePair<string, List<RenewSubscriptionDetails>> kvpRenewUnified in renewUnifiedDict)
            {
                List<RenewSubscriptionDetails> renewUnified = kvpRenewUnified.Value;

                double totalPrice = renewUnified.Sum(x => x.Price);
                string currency = renewUnified.Select(x => x.Currency).FirstOrDefault();
                int paymentMethodId = renewUnified.Select(x => x.PaymentMethodId).FirstOrDefault();
                try
                {
                    transactionResponse = Core.Billing.Module.ProcessUnifiedRenewal(groupId, householdId, totalPrice, currency, paymentgatewayId,
                        paymentMethodId, userIp, ref renewUnified, ref paymentGateway);

                    log.DebugFormat("Renew transaction returned from billing. data: {0}", logString);

                    if (transactionResponse == null || transactionResponse.Status == null)
                    {
                        // PG returned error
                        log.Error("Received error from Billing");
                        return false;
                    }

                    if (transactionResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Received error from Billing.ProcessRenewal code:{0}, msg:{1}", transactionResponse.Status.Code, transactionResponse.Status.Message);

                        if (transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotSetForHousehold ||
                            transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotExist ||
                            transactionResponse.Status.Code == (int)eResponseStatus.PaymentGatewayNotExist)
                        {
                            // renew subscription failed! pass 0 as failReasonCode since we don't get it on the transactionResponse
                            foreach (RenewSubscriptionDetails renewUnifiedData in renewUnified)
                            {
                                Subscription subscription = subscriptions.Where(x => x.m_SubscriptionCode == renewUnifiedData.ProductId).FirstOrDefault();
                                HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, renewUnifiedData, 0, transactionResponse.Status.Message,
                                    string.Empty, nextEndDate);
                            }
                            continue;
                        }
                    }

                    switch (transactionResponse.State)
                    {
                        case eTransactionState.OK:
                            {
                                HandleRenewUnifiedSubscriptionSuccess(cas, groupId, householdId, customData, ref unifiedBillingCycle, currency, subscriptions, renewUnified, paymentGateway);
                                successTransactions.Add(kvpRenewUnified.Key);
                            }
                            break;
                        case eTransactionState.Pending:
                        case eTransactionState.Failed:
                            {
                                // get tcm value 
                                int PendingThresholdDays = TCMClient.Settings.Instance.GetValue<int>("pending_threshold_days");
                                if (PendingThresholdDays == 0)
                                {
                                    PendingThresholdDays = PENDING_THRESHOLD_DAYS;
                                }

                                if (transactionResponse.State == eTransactionState.Failed || new DateTime(1970, 1, 1).AddMilliseconds(nextEndDate).AddDays(PendingThresholdDays) < DateTime.UtcNow)
                                {
                                    // renew subscription failed! pass 0 as failReasonCode since we don't get it on the transactionResponse
                                    foreach (RenewSubscriptionDetails renewUnifiedData in renewUnified)
                                    {
                                        Subscription subscription = subscriptions.Where(x => x.m_SubscriptionCode == renewUnifiedData.ProductId).FirstOrDefault();
                                        HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, renewUnifiedData, transactionResponse.FailReasonCode,
                                             null, string.Empty, nextEndDate);
                                    }

                                }
                                else
                                {
                                    pendingTransactions.Add(kvpRenewUnified.Key);
                                }

                            }
                            break;
                        default:
                            {
                                log.Error("Transaction state is unknown");
                            }
                            break;
                    }

                }
                catch (Exception ex)
                {
                    // error while trying to process renew in billing
                    log.Error("Error while calling the billing process renewal", ex);
                    return false;
                }
            }

            if (successTransactions.Count != 0 || pendingTransactions.Count != 0)
            {
                DateTime? successTransactionsEndDate = null;
                DateTime? pendingTransactionsEndDate = null;

                if (successTransactions.Count > 0)
                {
                    successTransactionsEndDate = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate);
                }

                if (pendingTransactions.Count > 0)
                {
                    pendingTransactionsEndDate = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(nextEndDate);
                }

                UpdateNextUnifiedCycle(groupId, householdId, paymentGateway, processId, processState, successTransactions, pendingTransactions, renewUnifiedDict, successTransactionsEndDate, pendingTransactionsEndDate);
            }
            else
            {
                bool changeUnifiedStatus = ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null);
            }

            return true; // no need to retry renew 
        }


        private static void UpdateNextUnifiedCycle(int groupId, long householdId, PaymentGateway paymentGateway, long processId, ProcessUnifiedState processState,
            List<string> successTransactions, List<string> pendingTransactions, Dictionary<string, List<RenewSubscriptionDetails>> renewUnifiedDict, DateTime? successTransactionsEndDate, DateTime? pendingTransactionsEndDate)
        {
            // insert right messages + update db

            try
            {
                if (successTransactions.Count > 0)
                {
                    string invalidationKey = LayeredCacheKeys.GetRenewInvalidationKey(householdId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on Renew key = {0}", invalidationKey);
                    }
                }

                switch (processState) // original state of this process
                {
                    case ProcessUnifiedState.Renew:
                        {
                            if (successTransactions.Count > 0)// some success
                            {
                                // update the success ones
                                UpdateSuccessTransactions(groupId, householdId, ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value);

                                // update the pandings 
                                if (pendingTransactions.Count > 0) // some are pending this renew process
                                {
                                    // get from db process for this with Pending State if exsits
                                    bool isNew = false;
                                    long pendingProcessId = Utils.GetUnifiedProcessId(groupId, paymentGateway.ID, pendingTransactionsEndDate.Value, householdId, out isNew, ProcessUnifiedState.Pending);
                                    if (pendingProcessId > 0)
                                    {
                                        // update subscription purchases table with process id
                                        List<int> purchaseIds = (renewUnifiedDict.Where(x => pendingTransactions.Contains(x.Key)).SelectMany(x => x.Value).ToList()).Select(y => (int)y.PurchaseId).ToList();
                                        ConditionalAccessDAL.UpdateMPPRenewalProcessId(purchaseIds, pendingProcessId);

                                        if (isNew)
                                        {
                                            HandleRenewUnifiedSubscriptionPending(groupId, householdId, pendingTransactionsEndDate.Value, paymentGateway.RenewalIntervalMinutes, pendingProcessId);
                                        }
                                    }
                                }
                            }
                            else // all are pending  
                            {
                                // update original state of this process to be Pending
                                ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, (int)ProcessUnifiedState.Pending);

                                // insert message to queue
                                HandleRenewUnifiedSubscriptionPending(groupId, householdId, pendingTransactionsEndDate.Value, paymentGateway.RenewalIntervalMinutes, processId);
                            }
                        }
                        break;
                    case ProcessUnifiedState.Pending:
                        {
                            if (pendingTransactions.Count > 0) // some still in pending status 
                            {
                                ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null);

                                // insert message to queue
                                HandleRenewUnifiedSubscriptionPending(groupId, householdId, pendingTransactionsEndDate.Value, paymentGateway.RenewalIntervalMinutes, processId);

                                // update the pandings success
                                if (successTransactions.Count > 0) // some are success this renew process
                                {
                                    bool isNew = false;
                                    long renewProcessId = Utils.GetUnifiedProcessId(groupId, paymentGateway.ID, successTransactionsEndDate.Value, householdId, out isNew, ProcessUnifiedState.Renew);
                                    if (renewProcessId > 0)
                                    {
                                        // update subscription purchases table with process id
                                        List<int> purchaseIds = (renewUnifiedDict.Where(x => successTransactions.Contains(x.Key)).SelectMany(x => x.Value).ToList()).Select(y => (int)y.PurchaseId).ToList();
                                        ConditionalAccessDAL.UpdateMPPRenewalProcessId(purchaseIds, renewProcessId);

                                        if (isNew)
                                        {
                                            UpdateSuccessTransactions(groupId, householdId, ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value, false);
                                        }
                                    }
                                }
                            }
                            else // all success
                            {
                                long successProcessId = 0;
                                DataTable dt = ConditionalAccessDAL.GetUnifiedProcessId(groupId, paymentGateway.ID, successTransactionsEndDate.Value, householdId, (int)ProcessUnifiedState.Renew);
                                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                {
                                    successProcessId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "id");
                                }

                                if (successProcessId > 0)
                                {
                                    // update subsuription purchase with success
                                    // update subscription purchases table with process id
                                    List<int> purchaseIds = (renewUnifiedDict.Where(x => successTransactions.Contains(x.Key)).SelectMany(x => x.Value).ToList()).Select(y => (int)y.PurchaseId).ToList();
                                    ConditionalAccessDAL.UpdateMPPRenewalProcessId(purchaseIds, successProcessId);
                                }
                                else // sucess process not exsits (use pendding process exsits in DB)
                                {
                                    UpdateSuccessTransactions(groupId, householdId, ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to UpdateNextUnifiedCycle", ex);
            }
        }

        private static bool UpdateSuccessTransactions(int groupId, long householdId, long endDate, PaymentGateway paymentGateway, long processId, DateTime endDatedt, bool updateUnifiedProcess = true)
        {
            bool saved = true;
            DateTime nextRenewalDate = endDatedt.AddMinutes(paymentGateway.RenewalStartMinutes);

            // insert message to queue
            Utils.RenewUnifiedTransactionMessageInQueue(groupId, householdId, endDate, nextRenewalDate, processId);

            if (updateUnifiedProcess)
            {
                // update date for next process
                saved = ConditionalAccessDAL.UpdateUnifiedProcess(processId, endDatedt, (int)ProcessUnifiedState.Renew);
            }

            return saved;
        }

        private static bool GetProcessDetails(long processId, ref int paymentgatewayId, ref ProcessUnifiedState processPurchasesState, ref DateTime? processEndDate)
        {
            bool result = false;
            DataRow dr = ConditionalAccessDAL.GetUnifiedProcessById(processId);
            if (dr != null)
            {
                paymentgatewayId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PAYMENT_GATEWAY_ID");
                int state = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATE");
                processPurchasesState = (ProcessUnifiedState)state;
                processEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                result = true;
            }

            return result;
        }

        private static bool HandleRenewUnifiedSubscriptionSuccess(BaseConditionalAccess cas, int groupId, long householdId, string customData, ref UnifiedBillingCycle unifiedBillingCycle,
            string currency, List<Subscription> subscriptions, List<RenewSubscriptionDetails> renewUnified,
            PaymentGateway paymentGateway)
        {
            DateTime? endDate = null;

            Subscription subscription = null;

            foreach (RenewSubscriptionDetails renewUnifiedData in renewUnified)
            {
                if (!endDate.HasValue)
                {
                    if (renewUnifiedData.SubscriptionStatus == SubscriptionPurchaseStatus.Suspended && renewUnifiedData.EndDate.Value < DateTime.UtcNow)
                    {
                        endDate = Utils.GetEndDateTime(DateTime.UtcNow, renewUnifiedData.MaxVLCOfSelectedUsageModule);
                    }
                    else
                    {
                        endDate = Utils.GetEndDateTime(renewUnifiedData.EndDate.Value, renewUnifiedData.MaxVLCOfSelectedUsageModule);
                    }

                    DateTime ubcDate = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate);

                    if (ubcDate < endDate)
                    {
                        log.DebugFormat("going to update UnifiedBillingCycle. current date = {0}, new Date = {1}", unifiedBillingCycle.endDate.ToString(), endDate.Value);

                        // update unified billing cycle for domian with next end date
                        long? groupBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
                        if (groupBillingCycle.HasValue)
                        {
                            long nextEndDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(endDate.Value);

                            if (unifiedBillingCycle.endDate < nextEndDate)
                            {
                                Utils.HandleDomainUnifiedBillingCycle(groupId, householdId, renewUnifiedData.MaxVLCOfSelectedUsageModule, nextEndDate);

                                unifiedBillingCycle.endDate = nextEndDate;
                            }
                        }
                    }
                }

                subscription = subscriptions.Where(x => x.m_SubscriptionCode == renewUnifiedData.ProductId).FirstOrDefault();

                // update MPP renew data
                try
                {
                    ConditionalAccessDAL.Update_MPPRenewalData(renewUnifiedData.PurchaseId, true, endDate.Value, 0, "CA_CONNECTION_STRING", renewUnifiedData.UserId, (int)SubscriptionPurchaseStatus.OK);
                    cas.WriteToUserLog(renewUnifiedData.UserId, string.Format("Successfully renewed. Product ID: {0}, price: {1}, currency: {2}, purchase ID: {3}, Billing Transition ID: {4}",
                        renewUnifiedData.ProductId,                           // {0}
                        renewUnifiedData.Price,                               // {1}
                        currency,                                             // {2}
                        renewUnifiedData.PurchaseId,                          // {3}
                        renewUnifiedData.BillingTransactionId));                  // {4}


                }
                catch (Exception ex)
                {
                    log.Error("Error while trying to update MPP renew data", ex);
                    return true;
                }

                // update compensation use
                if (renewUnifiedData.Compensation != null)
                {
                    if (!ConditionalAccessDAL.UpdateSubscriptionCompensationUse(renewUnifiedData.Compensation.Id, renewUnifiedData.BillingTransactionId, renewUnifiedData.Compensation.Renewals + 1))
                        log.ErrorFormat("Failed to update subscription compensation use. compensationId = {0}, billingTransactionId = {1}, renewalNumber = {2}",
                            renewUnifiedData.Compensation.Id, renewUnifiedData.BillingTransactionId, renewUnifiedData.Compensation.Renewals + 1);
                }

                // message for PS use
                Dictionary<string, object> psMessage = new Dictionary<string, object>()
                                        {
                                            {"BillingTransactionID", renewUnifiedData.BillingTransactionId},
                                            {"SiteGUID", renewUnifiedData.UserId},
                                            {"PaymentNumber", renewUnifiedData.PaymentNumber},
                                            {"TotalPaymentsNumber", renewUnifiedData.TotalNumOfPayments},
                                            {"CustomData", renewUnifiedData.CustomData},
                                            {"Price", renewUnifiedData.Price},
                                            {"PurchaseID", renewUnifiedData.PurchaseId},
                                            {"SubscriptionCode", subscription.m_SubscriptionCode}
                                        };

                cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, psMessage);
            }



            return true;
        }

        public static bool HandleRenewUnifiedSubscriptionPending(int groupId, long householdId, DateTime endDate, int renewalIntervalMinutes, long processID)
        {
            try
            {
                DateTime nextRenewalDate = DateTime.UtcNow.AddMinutes(renewalIntervalMinutes);

                // enqueue unified renew transaction
                Utils.RenewUnifiedTransactionMessageInQueue(groupId, householdId, ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(endDate), nextRenewalDate, processID);
            }
            catch (Exception ex)
            {
                log.Error("fail HandleRenewUnifiedSubscriptionPending ex={0}", ex);
            }
            return true;
        }

        private static bool HandleRenewUnifiedSubscriptionFailed(BaseConditionalAccess cas, int groupId, int paymentgatewayId, long householdId, Subscription subscription,
            RenewSubscriptionDetails spDetails, int failReasonCode, string billingSettingError = null, string billingGuid = null, long endDateUnix = 0)
        {
            string logString = string.Empty;

            logString = string.Format("Unified Purchase request (fail): householdId:{0}, siteguid {1}, purchaseId {2}, billingGuid {3}", 
                householdId, spDetails.UserId, spDetails.PurchaseId, spDetails.BillingGuid);

            return HandleRenewSubscriptionFailed(cas, groupId, spDetails.UserId, spDetails.PurchaseId, logString, long.Parse(spDetails.ProductId), 
                subscription, householdId, failReasonCode, billingSettingError, billingGuid, endDateUnix / 1000);
        }

        public static bool HandleResumeDomainSubscription(int groupId, long householdId, string siteguid, long purchaseId, string billingGuid, DateTime endDate)
        {
            try
            {
                log.DebugFormat("HandleResumeDomainSubscription. groupId: {0}, householdId: {1}, siteguid: {2}, purchaseId: {3}, billingGuid: {4}", groupId, householdId, siteguid, purchaseId, billingGuid);

                DateTime nextRenewalDate = DateTime.UtcNow;
                // enqueue renew transaction
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), nextRenewalDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction for resume domain {0}", data);
                    return false;
                }
                else
                {
                    PurchaseManager.SendRenewalReminder(data, householdId);
                    log.DebugFormat("New task created (upon renew pending response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        
        public static EntitlementRenewalResponse GetEntitlementNextRenewal(BaseConditionalAccess cas, int groupId, long householdId, long purchaseId)
        {
            EntitlementRenewalResponse response = new EntitlementRenewalResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // get subscription purchase 
            DataRow subscriptionRenealDataRow = DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseNextRenewal(groupId, purchaseId);

            // validate subscription received
            if (subscriptionRenealDataRow == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("GetEntitlementNextRenewal: failed subscription purchase. PurchaseId: {0}, householdId: {1}", purchaseId, householdId);
                return response;
            }

            // get product ID
            long productId = ODBCWrapper.Utils.ExtractInteger(subscriptionRenealDataRow, "SUBSCRIPTION_CODE"); // AKA subscription ID/CODE
            string couponCode = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "COUPON_CODE");
            string billingGuid = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "billing_Guid");

            // Check if this is a renew via INAPP PURCHASE
            PaymentDetails pd = null;
            ApiObjects.Response.Status statusVerifications = Billing.Module.GetPaymentGatewayVerificationStatus(groupId, billingGuid, ref pd);
            bool ignoreUnifiedBillingCycle = statusVerifications.Code != (int)eResponseStatus.OK || pd == null || pd.PaymentGatewayId == 0;

            // get end date
            DateTime endDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionRenealDataRow, "END_DATE");
           
            string previousPurchaseCurrencyCode = string.Empty;
            string previousPurchaseCountryCode = string.Empty;
            string previousPurchaseCountryName = string.Empty;


            string customData = null;
            #region Dummy
            try
            {
                customData = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "CUSTOMDATA"); // AKA subscription ID/CODE

                if (!string.IsNullOrEmpty(customData))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(customData);
                    XmlNode theRequest = doc.FirstChild;
                    // previousPurchaseCurrencyCode, previousPurchaseCountryCode and previousPurchaseCountryName will be used later for getting the correct priceCodeData 
                    previousPurchaseCurrencyCode = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                    previousPurchaseCountryName = XmlUtils.GetSafeValue(BaseConditionalAccess.COUNTRY_CODE, ref theRequest);
                    previousPurchaseCountryCode = Utils.GetCountryCodeByCountryName(groupId, previousPurchaseCountryName);
                    bool isDummy = XmlUtils.IsNodeExists(ref theRequest, BaseConditionalAccess.DUMMY);
                    if (isDummy)
                    {
                        // OK + price 0
                        response = new EntitlementRenewalResponse()
                        {
                            EntitlementRenewal = new EntitlementRenewal()
                            {
                                Price = new Price()
                                {
                                    m_dPrice = 0
                                },
                                SubscriptionId = productId,
                                PurchaseId = purchaseId,
                                Date = endDate,
                                GroupId = groupId,
                                Id = purchaseId
                            },
                            Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
                        };

                        return response;
                    }
                } 
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEntitlementNextRenewal: Error while getting data from custom data xml, PurchaseId: {0}, householdId: {1}", purchaseId, householdId), ex);
                return response;
            }

            #endregion
            
            #region Get Subscription data

            Subscription subscription = null;
            try
            {
                subscription = Core.Pricing.Module.GetSubscriptionData(groupId, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEntitlementNextRenewal: Error while trying to fetch subscription data. PurchaseId: {0}, householdId: {1}", purchaseId, householdId), ex);
                return response;
            }


            // validate subscription
            if (subscription == null)
            {
                // subscription wasn't found
                log.ErrorFormat("GetEntitlementNextRenewal: subscription wasn't found. productId {0}, PurchaseId: {1}, householdId: {2}", productId, purchaseId, householdId);
                return response;
            }

            #endregion

            if (subscription.Type == SubscriptionType.AddOn && subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
            {
                ApiObjects.Response.Status status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("GetEntitlementNextRenewal: subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}", subscription.m_SubscriptionCode, status.Code, status.Message);
                    response.Status = status;
                    return response;
                }
            }

            int paymentNumber = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRenealDataRow, "PAYMENT_NUMBER");
            int numOfPayments = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRenealDataRow, "number_of_payments");
            int totalNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRenealDataRow, "total_number_of_payments");

            // check if purchased with preview module
            bool isPurchasedWithPreviewModule;
            isPurchasedWithPreviewModule = ApiDAL.Get_IsPurchasedWithPreviewModuleByBillingGuid(groupId, billingGuid, (int)purchaseId);

            paymentNumber = Utils.CalcPaymentNumber(numOfPayments, paymentNumber, isPurchasedWithPreviewModule);
            if (numOfPayments > 0 && paymentNumber > numOfPayments)
            {
                // Subscription ended
                log.ErrorFormat("GetEntitlementNextRenewal: Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}", numOfPayments, paymentNumber, numOfPayments);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            // calculate payment number
            paymentNumber++;

            // get compensation data
            Compensation compensation = ConditionalAccessDAL.GetSubscriptionCompensationByPurchaseId(purchaseId);

            // get MPP
            int recPeriods = 0;
            bool isMPPRecurringInfinitely = false;
            int maxVLCOfSelectedUsageModule = 0;
            double price = 0;
            string currency = "n/a";
            UnifiedBillingCycle unifiedBillingCycle = null;
            string siteguid = "";
            string userIp = "";

            if (!cas.GetMultiSubscriptionUsageModule(siteguid, userIp, (int)purchaseId, paymentNumber, totalNumOfPayments, numOfPayments, isPurchasedWithPreviewModule,
                    ref price, ref customData, ref currency, ref recPeriods, ref isMPPRecurringInfinitely, ref maxVLCOfSelectedUsageModule,
                    ref couponCode, subscription, ref unifiedBillingCycle, compensation, previousPurchaseCountryName, previousPurchaseCountryCode, previousPurchaseCurrencyCode,
                    endDate, groupId, householdId, ignoreUnifiedBillingCycle, false))
            {
                log.Error("Error while trying to get Price plan to renew");
                return response;
            }

            var currencyObj = Core.Pricing.Module.GetCurrencyValues(groupId, currency);
            ApiObjects.Country country = null;
            if (!string.IsNullOrEmpty(previousPurchaseCountryName))
            {
                country = Utils.GetCountryByCountryName(groupId, previousPurchaseCountryName);
            }

            response.EntitlementRenewal = new EntitlementRenewal()
            {
                Date = endDate,
                SubscriptionId = productId,
                PurchaseId = purchaseId,
                Price = new Price()
                {
                    m_dPrice = price,
                    m_oCurrency = currencyObj,
                    countryId = country != null ? country.Id : 0
                },
                GroupId = groupId,
                Id = purchaseId
            };

            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public static UnifiedPaymentRenewalResponse GetUnifiedPaymentNextRenewal(BaseConditionalAccess cas, int groupId, long householdId, long unifiedPaymentId)
        {
            UnifiedPaymentRenewalResponse response = new UnifiedPaymentRenewalResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            string customData = string.Empty;

            long? groupBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
            if (!groupBillingCycle.HasValue)
            {
                log.DebugFormat("GetUnifiedPaymentRenewal: failed to get groupBillingCycle from cache for groupId={0}", groupId);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Unified payment is not set for partner");
                return response;
            }

            int paymentgatewayId = 0;
            ProcessUnifiedState processState = ProcessUnifiedState.Renew;
            DateTime? processEndDate = null;
            bool result = GetProcessDetails(unifiedPaymentId, ref paymentgatewayId, ref processState, ref processEndDate);

            UnifiedBillingCycle unifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle((int)householdId, groupBillingCycle.Value);
            if (unifiedBillingCycle == null)
            {
                log.DebugFormat("GetUnifiedPaymentRenewal: unifiedBillingCycle not found in CB for householdId ={0}", householdId);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Unified payment is not set for household");
                return response;
            }

            // get subscription purchase 
            DataTable subscriptionPurchaseDt = DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseUnifiedForRenewal(groupId, householdId, unifiedPaymentId);

            // validate subscription received
            if (subscriptionPurchaseDt == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("GetUnifiedPaymentRenewal: failed to get unifiedBillingCycle from CB for householdId ={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }

            List<RenewSubscriptionDetails> renewSubscriptioDetails = Utils.BuildSubscriptionPurchaseDetails(subscriptionPurchaseDt, groupId, cas);
            if (renewSubscriptioDetails == null || renewSubscriptioDetails.Count() == 0)// all resDetails were addon with none relevant base
            {
                log.DebugFormat("GetUnifiedPaymentRenewal: No subscriptions found for householdId={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }

            List<long> purchasesIds = renewSubscriptioDetails.Select(s => s.PurchaseId).ToList();

            List<Subscription> subscriptions = null;
            try
            {
                subscriptions = Utils.GetSubscriptionsDataWithCaching(renewSubscriptioDetails.Select(x => x.ProductId).ToList(), groupId).ToList();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. data: householdId={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId), ex);
                return response;
            }

            // validate subscription
            if (subscriptions == null || subscriptions.Count() == 0)
            {
                // subscription wasn't found
                log.Error(string.Format("GetUnifiedPaymentRenewal: Failed to get subscriptions data from cache. productIds {0}", string.Join(",", renewSubscriptioDetails.Select(x => x.ProductId).ToList())));
                return response;
            }

            #region check addon subscriptions
            List<string> removeSubscriptionCodes = new List<string>(); 
            List<Subscription> baseSubscriptions = subscriptions.Where(x => x.Type == SubscriptionType.Base).ToList();
            foreach (Subscription subscription in (subscriptions.Where(x => x.Type == SubscriptionType.AddOn).ToList()))
            {
                if (subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
                {
                    ApiObjects.Response.Status status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        // check mabye this add on have base subscription in this unified billing cycle 
                        bool canPurchaseAddOn = false;
                        // get all setsIds for this addon 
                        List<long> addOnSetIds = subscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();
                        // check if one of the subscription are base in this unified cycle 
                        foreach (Subscription baseSubscription in baseSubscriptions)
                        {
                            List<long> baseSetIds = baseSubscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();
                            if (baseSetIds.Where(x => addOnSetIds.Contains(x)).Count() > 0)
                            {
                                canPurchaseAddOn = true;
                            }
                        }
                        if (!canPurchaseAddOn)
                        {
                            // change is recurring to false and call event handle- this renew subscription failed!                        

                            long nextEndDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(processEndDate.Value);

                            RenewSubscriptionDetails rsDetail = renewSubscriptioDetails.Where(x => x.ProductId == subscription.m_SubscriptionCode).FirstOrDefault();
                            if (HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, rsDetail, 0, "AddOn with no BaseSubscription valid",
                                string.Empty, nextEndDate))
                            {
                                // save all SubscriptionCode to remove from subscription list 
                                removeSubscriptionCodes.Add(subscription.m_SubscriptionCode);
                                // remove this renewDetails (its an AddOn)
                                renewSubscriptioDetails.Remove(rsDetail);
                            }

                            log.ErrorFormat("failed renew subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}", subscription.m_SubscriptionCode, status.Code, status.Message);
                        }
                    }
                }
            }

            if (renewSubscriptioDetails == null || renewSubscriptioDetails.Count() == 0)// all resDetails were addon with none relevant base
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no addon subscriptions for householdid = {0} and paymentgatewayid = { 1 }", householdId, paymentgatewayId);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            if (removeSubscriptionCodes.Count() > 0) // remove all not relevant subscriptions
            {
                subscriptions.RemoveAll(x => removeSubscriptionCodes.Contains(x.m_SubscriptionCode));
            }
            #endregion

            Utils.GetMultiSubscriptionUsageModule(renewSubscriptioDetails, string.Empty, subscriptions, cas, ref unifiedBillingCycle, (int)householdId, groupId, false);

            if (renewSubscriptioDetails == null || renewSubscriptioDetails.Count == 0)
            {
                log.ErrorFormat("GetUnifiedPaymentRenewal: Failed to get renewal data for householdId={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }
            // build response 
            var currencyObj = Core.Pricing.Module.GetCurrencyValues(groupId, renewSubscriptioDetails[0].Currency);
            ApiObjects.Country country = null;
            if (!string.IsNullOrEmpty(renewSubscriptioDetails[0].CountryName))
            {
                country = Utils.GetCountryByCountryName(groupId, renewSubscriptioDetails[0].CountryName);
            }

            response.UnifiedPaymentRenewal = new UnifiedPaymentRenewal()
            {
                Date = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate),
                UnifiedPaymentId = unifiedPaymentId,
                Entitlements = new List<EntitlementRenewalBase>(),
                Price = new Price()
                {
                    m_oCurrency = currencyObj,
                    countryId = country != null ? country.Id : 0
                },
                GroupId = groupId,
                Id = unifiedPaymentId
            };

            EntitlementRenewalBase entitlementRenewal = null;
            foreach (var subDetails in renewSubscriptioDetails)
            {
                entitlementRenewal = new EntitlementRenewalBase()
                {
                    PriceAmount = subDetails.Price,
                    PurchaseId = subDetails.PurchaseId,
                    SubscriptionId = long.Parse(subDetails.ProductId),
                    GroupId = groupId,
                    Id = subDetails.PurchaseId
                };
                response.UnifiedPaymentRenewal.Entitlements.Add(entitlementRenewal);
                response.UnifiedPaymentRenewal.Price.m_dPrice += subDetails.Price;
            }

            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response; 
        }

        internal static bool RenewalReminder(BaseConditionalAccess baseConditionalAccess, int groupId, string siteGuid, long purchaseId, long nextEndDate)
        {
            bool success = false;

            // log request
            string logString = string.Format("RenewalReminder request: userId {0}, purchaseId {1}, nextEndDate {2}", siteGuid, purchaseId, nextEndDate);

            log.DebugFormat("Starting RenewalReminder process. data: {0}", logString);

            long householdId = 0;

            // validate purchaseId
            if (purchaseId <= 0)
            {
                // Illegal purchase ID  
                log.ErrorFormat("Illegal purchaseId. data: {0}", logString);
                return true;
            }

            var renewalResponse = GetEntitlementNextRenewal(baseConditionalAccess, groupId, householdId, purchaseId);

            if (renewalResponse == null || renewalResponse.Status == null || renewalResponse.Status.Code != (int)eResponseStatus.OK || renewalResponse.EntitlementRenewal == null)
            {
                log.ErrorFormat("Error when getting entitlement renewal data for purchaseId {0}", purchaseId);
                return true;
            }

            Domain domain;
            User user;
            var userValidStatus = Utils.ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain, out user);

            // validate household
            if ((userValidStatus == null || userValidStatus.Code != (int)eResponseStatus.OK) &&
                householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }

            // get end date
            DateTime endDate = renewalResponse.EntitlementRenewal.Date.ToUniversalTime();

            // validate renewal did not already happen
            if (Math.Abs(TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate) - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling RenewalReminder task." +
                    "Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), logString);
                return true;
            }

            renewalResponse.EntitlementRenewal.Price.m_oCurrency.m_sCurrencySign = string.Empty;

            success = renewalResponse.EntitlementRenewal.Notify();
            
            return success;
        }

        internal static bool UnifiedRenewalReminder(BaseConditionalAccess baseConditionalAccess, int groupId, string siteGuid, long householdId, long processId, long nextEndDate)
        {
            bool success = false;

            // log request
            string logString = string.Format("RenewalReminder request: userId {0}, processId {1}, endDateLong {2}", siteGuid, processId, nextEndDate);

            log.DebugFormat("Starting UnifiedRenewalReminder process. data: {0}", logString);

            // validate purchaseId
            if (processId <= 0)
            {
                // Illegal purchase ID  
                log.ErrorFormat("Illegal processId. data: {0}", logString);
                return true;
            }

            if (!string.IsNullOrEmpty(siteGuid))
            {
                Domain domain;
                User user;
                var userValidStatus = Utils.ValidateUserAndDomain(groupId, siteGuid, ref householdId, out domain, out user);

                // validate household
                if ((userValidStatus == null || userValidStatus.Code != (int)eResponseStatus.OK) &&
                    householdId <= 0)
                {
                    // illegal household ID
                    log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                    return true;
                }
            }
            else if (householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }

            var unifiedPaymentResponse = GetUnifiedPaymentNextRenewal(baseConditionalAccess, groupId, householdId, processId);

            if (unifiedPaymentResponse == null || unifiedPaymentResponse.Status == null || unifiedPaymentResponse.Status.Code != (int)eResponseStatus.OK || 
                unifiedPaymentResponse.UnifiedPaymentRenewal == null)
            {
                log.ErrorFormat("Error when getting entitlement renewal data for processId {0}", processId);
                return true;
            }

            unifiedPaymentResponse.UnifiedPaymentRenewal.Price.m_oCurrency.m_sCurrencySign = string.Empty;

            success = unifiedPaymentResponse.UnifiedPaymentRenewal.Notify();

            return success;
        }

        private static bool UpdateProcessDetailsForRenewal(long processId, ref int paymentgatewayId, ref ProcessUnifiedState processPurchasesState, ref DateTime? processEndDate)
        {
            DataRow dr = ConditionalAccessDAL.UpdateProcessDetailsForRenewal(processId);
            if (dr != null)
            {
                paymentgatewayId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PAYMENT_GATEWAY_ID");
                int state = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATE");
                processPurchasesState = (ProcessUnifiedState)state;
                processEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
            }

            return paymentgatewayId > 0 && processEndDate.HasValue;
        }
    }
}
