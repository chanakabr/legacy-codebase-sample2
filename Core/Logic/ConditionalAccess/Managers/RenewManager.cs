using APILogic.ConditionalAccess.Managers;
using APILogic.ConditionalAccess.Modules;
using APILogic.ConditionalAccess.Response;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.ConditionalAccess.Modules;
using Core.Pricing;
using Core.Users;
using DAL;
using Phx.Lib.Log;
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
            string logString = string.Format("Renew Purchase request: siteguid {0}, purchaseId {1}, billingGuid {2}, nextEndDate {3}", siteguid, purchaseId, billingGuid, nextEndDate);
            log.DebugFormat("Starting renewal process. data: {0}", logString);
            long domainId = 0;
            string userIp = "1.1.1.1";

            // validate purchaseId
            if (purchaseId <= 0 || string.IsNullOrEmpty(billingGuid))
            {
                log.ErrorFormat("Illegal purchaseId or billingGuid. data: {0}", logString);
                return true;
            }

            log.DebugFormat("Renew details received. data: {0}", logString);

            ResponseStatus userValidStatus = ResponseStatus.OK;
            userValidStatus = Utils.ValidateUser(groupId, siteguid, ref domainId, true);

            var renewDetailsResponse = GetRenewDetails(cas, logString, userValidStatus, domainId, siteguid, purchaseId, groupId, billingGuid, ref shouldUpdateTaskStatus, out Subscription subscription, userIp);
            if (renewDetailsResponse.Object == null)
            {
                if (renewDetailsResponse.Status.Code == (int)eResponseStatus.OK) { return true; }
                return false;
            }
            var renewDetails = renewDetailsResponse.Object;

            // Check if this is a renew via INAPP PURCHASE
            PaymentDetails paymentDetails = null;
            ApiObjects.Response.Status statusVerifications = Billing.Module.GetPaymentGatewayVerificationStatus(groupId, billingGuid, ref paymentDetails);
            bool ignoreUnifiedBillingCycle = statusVerifications.Code != (int)eResponseStatus.OK || paymentDetails == null || paymentDetails.PaymentGatewayId == 0;

            if (statusVerifications.Code != (int)eResponseStatus.PaymentGatewayNotValid &&
                !APILogic.Api.Managers.RolesPermissionsManager.Instance.IsPermittedPermission(groupId, renewDetails.UserId, RolePermissions.RENEW_SUBSCRIPTION))
            {
                // mark this subscription in special status 
                if (!ConditionalAccessDAL.UpdateMPPRenewalSubscriptionStatus(new List<int>() { (int)purchaseId }, (int)SubscriptionPurchaseStatus.Suspended))
                {
                    log.ErrorFormat("Failed to suspend purchase id entitlements for payment gateway: UpdateMPPRenewalSubscriptionStatus fail in DB purchaseId={0}, householdId={1}", purchaseId, domainId);
                }

                log.ErrorFormat("domain is not permitted to renew process. Data:{0}", logString);
                return true;
            }

            // get end date
            var endDateUnix = renewDetails.EndDate.Value.ToUtcUnixTimestampSeconds();

            // validate renewal did not already happened
            if (Math.Abs(endDateUnix - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Canceling renew task because subscription purchase:{0} last_end_date:{1} is not the same as the next_end_date:{2}. data:{3}",
                                 purchaseId, endDateUnix, nextEndDate, logString);
                return true;
            }

            log.DebugFormat("subscription purchase found and validated. data: {0}", logString);

            if (subscription.Type == SubscriptionType.AddOn && subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
            {
                var status = Utils.CanPurchaseAddOn(groupId, renewDetails.DomainId, subscription);
                if (!status.IsOkStatusCode())
                {
                    // change is recurring to false and call event handle renew subscription failed!
                    var billingSettingError = "AddOn with no BaseSubscription valid";
                    HandleRenewSubscriptionFailed(cas, renewDetails, logString, subscription, 0, billingSettingError, endDateUnix);

                    log.ErrorFormat("failed renew subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}",
                                    subscription.m_SubscriptionCode, status.Code, status.Message);
                    return true;
                }
            }

            // check if purchased with preview module
            if (renewDetails.NumOfPayments > 0 && renewDetails.PaymentNumber > renewDetails.NumOfPayments)
            {
                // Subscription ended
                var subscriptionEndedLog = string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, billingGuid={3}",
                                                         renewDetails.ProductId, renewDetails.NumOfPayments, renewDetails.PaymentNumber, billingGuid);
                log.Error(subscriptionEndedLog);
                cas.WriteToUserLog(siteguid, subscriptionEndedLog);
                return true;
            }

            // calculate payment number
            renewDetails.PaymentNumber++;

            // get MPP
            int recPeriods = 0;
            bool isMPPRecurringInfinitely = false;
            SubscriptionCycle subscriptionCycle = null;
            if (!cas.GetMultiSubscriptionUsageModule(renewDetails, userIp, ref recPeriods, ref isMPPRecurringInfinitely, subscription,
                                                     ref subscriptionCycle, groupId, ignoreUnifiedBillingCycle))
            {
                // "Error while trying to get Price plan
                log.Error("Error while trying to get Price plan to renew");
                return false;
            }

            // call billing process renewal
            TransactResult transactionResponse = null;
            try
            {
                renewDetails.BillingGuid = billingGuid;
                renewDetails.GracePeriodMinutes = subscription.m_GracePeriodMinutes;

                transactionResponse = Billing.Module.ProcessRenewal(renewDetails, subscription.m_ProductCode, subscription.ExternalProductCodes);
                
                if (transactionResponse == null || transactionResponse.Status == null)
                {
                    // PG returned error
                    log.Error("Received error from Billing");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // error while trying to process renew in billing
                log.Error("Error while calling the billing process renewal", ex);
                return false;
            }

            log.DebugFormat("Renew transaction returned from billing. data: {0}", logString);

            if (transactionResponse.Status.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Received error from Billing.ProcessRenewal code:{0}, msg:{1}", transactionResponse.Status.Code, transactionResponse.Status.Message);

                if (transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotSetForHousehold ||
                    transactionResponse.Status.Code == (int)eResponseStatus.PaymentMethodNotExist ||
                    transactionResponse.Status.Code == (int)eResponseStatus.PaymentGatewayNotExist)
                {
                    // renew subscription failed! pass 0 as failReasonCode since we don't get it on the transactionResponse
                    return HandleRenewSubscriptionFailed(cas, renewDetails, logString, subscription, 0, transactionResponse.Status.Message, endDateUnix);
                }

                return false;
            }

            bool res = false;
            switch (transactionResponse.State)
            {
                case eTransactionState.OK:
                    {
                        UnifiedProcess unifiedProcess = null;
                        if (!ignoreUnifiedBillingCycle && subscriptionCycle != null) //should be part of unified cycle 
                        {
                            var paymentGatewayId = paymentDetails != null ? paymentDetails.PaymentGatewayId : 0;
                            unifiedProcess = UpdateMPPRenewalProcessId(groupId, renewDetails, subscriptionCycle, paymentGatewayId);
                        }

                        res = HandleRenewSubscriptionSuccess(cas, renewDetails, logString, subscription, transactionResponse, subscriptionCycle, unifiedProcess);

                        if (res)
                        {
                            string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, domainId);
                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                            {
                                log.ErrorFormat("Failed to set invalidation key on Renew key = {0}", invalidationKey);
                            }
                        }
                    }
                    break;
                case eTransactionState.Pending:
                    {
                        // check if to update siteGuid in subscription_purchases to masterSiteGuid and set renewal to masterSiteGuid
                        if (renewDetails.ShouldSwitchToMasterUser)
                        {
                            ConditionalAccessDAL.Update_SubscriptionPurchaseRenewalSiteGuid(renewDetails.GroupId, renewDetails.BillingGuid, (int)renewDetails.PurchaseId, renewDetails.UserId, "CA_CONNECTION_STRING");
                        }

                        var response = Billing.Module.GetPaymentGateway(groupId, domainId, paymentDetails.PaymentGatewayId, string.Empty);

                        if (response == null || response.PaymentGateway == null || response.PaymentGateway.ID == 0)
                        {

                        }
                        else
                        {
                            //Mark purchase as suspend
                            if (response.Status.Code == (int)eResponseStatus.PaymentGatewaySuspended && renewDetails.SubscriptionStatus != SubscriptionPurchaseStatus.Suspended)
                            {
                                if (!ConditionalAccessDAL.UpdateMPPRenewalSubscriptionStatus(new List<int>() { (int)purchaseId }, (int)SubscriptionPurchaseStatus.Suspended))
                                {
                                    log.ErrorFormat("Failed to suspend purchase id  entitlements for payment gateway: UpdateMPPRenewalSubscriptionStatus fail in DB purchaseId={0}, householdId={1}", purchaseId, renewDetails.DomainId);
                                }
                            }

                            DateTime nextRenewalDate = DateTime.UtcNow.AddMinutes(60); // default
                            int pendingInterval = transactionResponse.PendingInterval != 0
                                ? transactionResponse.PendingInterval
                                : response.PaymentGateway.RenewalIntervalMinutes;
                            log.Info($"Renew pending subscription in {pendingInterval} min");
                            if (pendingInterval > 0)
                            {
                                nextRenewalDate = DateTime.UtcNow.AddMinutes(pendingInterval);
                            }

                            UnifiedProcess unifiedProcess = null;
                            if (!ignoreUnifiedBillingCycle && subscriptionCycle != null && subscriptionCycle.UnifiedBillingCycle != null) //should be part of unified cycle 
                            {
                                res = true;
                                unifiedProcess = UpdateMPPRenewalProcessId(groupId, renewDetails, subscriptionCycle, paymentDetails.PaymentGatewayId);
                                    //GetRenewalProcessId(groupId, domainId, paymentDetails.PaymentGatewayId);
                                if (unifiedProcess != null && unifiedProcess.isNew)
                                {
                                    //todo create unified msg
                                    res = HandleRenewUnifiedSubscriptionPending(groupId, domainId, unifiedProcess.EndDate, response.PaymentGateway.RenewalIntervalMinutes, unifiedProcess.Id);
                                }
                            }
                            else
                            {
                                // renew subscription pending!
                                res = HandleRenewSubscriptionPending(cas, renewDetails, nextRenewalDate, logString);
                            }
                        }
                    }
                    break;
                case eTransactionState.Failed:
                    {
                        // renew subscription failed!
                        res = HandleRenewSubscriptionFailed(cas, renewDetails, logString, subscription, transactionResponse.FailReasonCode, null, endDateUnix);
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

        /// <summary>
        /// get right process id from DB and update this row in DB 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="purchaseId"></param>
        /// <param name="householdId"></param>
        /// <param name="subscriptionCycle"></param>
        /// <param name="paymentGatewayId"></param>
        /// <param name="maxVLCOfSelectedUsageModule"></param>
        /// <returns></returns>
        private static UnifiedProcess UpdateMPPRenewalProcessId(int groupId, RenewDetails renewDetails, SubscriptionCycle subscriptionCycle, int paymentGatewayId)
        {   
            UnifiedProcess unifiedProcess = null;

            try
            {
                var renewDuration = new Duration(renewDetails.MaxVLCOfSelectedUsageModule);
                if (subscriptionCycle.HasCycle && subscriptionCycle.SubscriptionLifeCycle.Equals(renewDuration))
                {
                    if (renewDetails.IsAddToUnified && subscriptionCycle.UnifiedBillingCycle != null)
                    {
                        unifiedProcess = GetRenewalProcessId(groupId, renewDetails.DomainId, paymentGatewayId, renewDetails.MaxVLCOfSelectedUsageModule, subscriptionCycle.UnifiedBillingCycle.endDate);
                    }

                    if (unifiedProcess == null) // need to create new process id 
                    {
                        renewDetails.EndDate = Utils.GetEndDateTime(renewDetails.EndDate.Value, renewDetails.MaxVLCOfSelectedUsageModule);                     

                        long processId = ConditionalAccessDAL.InsertUnifiedProcess(groupId, paymentGatewayId, renewDetails.EndDate.Value, renewDetails.DomainId, 
                            renewDetails.MaxVLCOfSelectedUsageModule, (int)ProcessUnifiedState.Renew);
                        
                        if (processId > 0)
                        {
                            unifiedProcess = new UnifiedProcess()
                            {
                                Id = processId,
                                EndDate = renewDetails.EndDate.Value,
                                isNew = true
                            };
                        }
                    }

                    if (unifiedProcess?.Id > 0)
                    {
                        // update subscription Purchase
                        DAL.ConditionalAccessDAL.UpdateMPPRenewalProcessId(new List<int>() { (int)renewDetails.PurchaseId }, unifiedProcess.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"fail update process id for specific renew groupId={groupId}, purchaseId={renewDetails.PurchaseId}, DomainId={renewDetails.DomainId}", ex);
            }
            return unifiedProcess;
        }

        private static UnifiedProcess GetRenewalProcessId(int groupId, long householdId, int paymentGatewayId, long cycle, long cycleDate = 0)
        {
            UnifiedProcess unifiedProcess = null;
            try
            {
                if (paymentGatewayId > 0)
                {
                    DataTable dt = DAL.ConditionalAccessDAL.GetUnifiedProcessIdByHouseholdPaymentGateway(groupId, paymentGatewayId, householdId, cycle);

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            int count = ODBCWrapper.Utils.GetIntSafeVal(row, "sp_count");

                            if (count == 0)
                            {
                                var endDate = ODBCWrapper.Utils.GetDateSafeVal(row, "END_DATE");
                                
                                if (cycleDate == DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate))
                                {
                                    unifiedProcess = new UnifiedProcess()
                                    {
                                        Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID"),
                                        EndDate = endDate,
                                    };

                                    log.Debug($"BEO-9166 GetRenewalProcessId (sp_count=0) {unifiedProcess.Id}");
                                }
                                
                            }
                            else
                            {
                                unifiedProcess = new UnifiedProcess()
                                {
                                    Id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID"),
                                    EndDate = ODBCWrapper.Utils.GetDateSafeVal(row, "END_DATE"),
                                };
                                break;
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetRenewalProcessId householdId:{0}, paymentGatewayId:{1}, ex:{2}", householdId, paymentGatewayId, ex);
            }
            return unifiedProcess;
        }

        protected internal static bool HandleRenewSubscriptionFailed(BaseConditionalAccess cas, RenewDetails renewDetails, string logString, Subscription subscription, int failReasonCode, string billingSettingError = null, long endDateUnix = 0)
        {
            log.DebugFormat("Transaction renew failed. data: {0}", logString);

            // grant entitlement
            SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(renewDetails.GroupId)
            {
                purchaseId = (int)renewDetails.PurchaseId,
                siteGuid = renewDetails.UserId,
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

            cas.WriteToUserLog(renewDetails.UserId, string.Format("Transaction renew failed. Product ID: {0}, purchase ID: {1}", renewDetails.ProductId, renewDetails.PurchaseId));

            // PS message 
            var dicData = new Dictionary<string, object>()
            {
                {"SiteGUID", renewDetails.UserId},
                {"DomainID", renewDetails.DomainId},
                {"FailReasonCode", failReasonCode},
                {"PurchaseID", renewDetails.PurchaseId},
                {"SubscriptionCode", subscription.m_SubscriptionCode}
            };

            if (failReasonCode == 0)
            {
                dicData.Add("BillingSettingError", billingSettingError);
            }

            cas.EnqueueEventRecord(NotifiedAction.FailedSubscriptionRenewal, dicData, renewDetails.UserId, string.Empty, string.Empty);

            // Enqueue event for subscription ends:
            EnqueueSubscriptionEndsMessage(renewDetails.GroupId, renewDetails.UserId, renewDetails.PurchaseId, endDateUnix);

            return true;
        }

        internal static void EnqueueSubscriptionEndsMessage(int groupId, string siteguid, long purchaseId, long endDateUnix)
        {
            DateTime endDate = DateUtils.UtcUnixTimestampSecondsToDateTime(endDateUnix);

            if (endDate > DateTime.Now.AddYears(1))
            {
                //BEO-11219
                log.Debug($"BEO-11219 - skip Enqueue subscription ends msg (more then 1 year)! purchaseId:{purchaseId}, endDateUnix:{endDateUnix}");
                return;
            }

            bool enqueueSuccessful = true;

            var queue = new RenewTransactionsQueue();
            var data = new RenewTransactionData(groupId, siteguid, purchaseId, string.Empty,
                            endDateUnix, endDate, eSubscriptionRenewRequestType.SubscriptionEnds);

            enqueueSuccessful &= queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of subscription ends event {0}", data);
            }
        }

        protected internal static bool HandleRenewSubscriptionPending(BaseConditionalAccess cas, RenewDetails renewDetails, DateTime nextRenewalDate, string logString)
        {
            log.DebugFormat("Transaction renew pending. data: {0}", logString);

            try
            {
                // enqueue renew transaction
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                RenewTransactionData data = new RenewTransactionData(renewDetails, DateUtils.DateTimeToUtcUnixTimestampSeconds(renewDetails.EndDate.Value), nextRenewalDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, renewDetails.GroupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                    return false;
                }
                else
                {
                    PurchaseManager.SendRenewalReminder(data, renewDetails.DomainId);
                    log.DebugFormat("New task created (upon renew pending response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }

            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get the PG", ex);
                return false;
            }

            var pendingLog = string.Format("pending renew returned. ProductId:{0}, price:{1}, currency:{2}, userID:{3}, PurchaseId:{4}.",
                                            renewDetails.ProductId, renewDetails.Price, renewDetails.Currency, renewDetails.UserId, renewDetails.PurchaseId);
            log.DebugFormat(pendingLog);
            cas.WriteToUserLog(renewDetails.UserId, pendingLog);
            return true;
        }

        protected static bool HandleRenewSubscriptionSuccess(BaseConditionalAccess cas, RenewDetails renewDetails, string logString,
            Subscription subscription, TransactResult transactionResponse, SubscriptionCycle subscriptionCycle, UnifiedProcess unifiedProcess)
        {
            // renew subscription success!
            log.DebugFormat("Transaction renew success. data: {0}", logString);

            PaymentGateway paymentGateway;

            try
            {
                // get billing gateway
                paymentGateway = Billing.Module.GetPaymentGatewayByBillingGuid(renewDetails.GroupId, renewDetails.DomainId, renewDetails.BillingGuid);
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
                renewDetails.EndDate = DateUtils.UtcUnixTimestampSecondsToDateTime(transactionResponse.EndDateSeconds);
                log.DebugFormat("New end-date was updated according to PG. EndDate={0}", renewDetails.EndDate);
            }
            // update unified billing cycle for domian with next end date
            else if (unifiedProcess?.Id > 0)
            {
                if (subscriptionCycle == null)
                {
                    subscriptionCycle = Utils.GetSubscriptionCycle(renewDetails.GroupId, (int)renewDetails.DomainId, renewDetails.MaxVLCOfSelectedUsageModule);
                }
                
                if (subscriptionCycle.HasCycle)
                {
                    if (subscriptionCycle.UnifiedBillingCycle == null)
                    {
                        subscriptionCycle.UnifiedBillingCycle = UnifiedBillingCycleManager.GetDomainUnifiedBillingCycle((int)renewDetails.DomainId, renewDetails.MaxVLCOfSelectedUsageModule);
                    }

                    if (subscriptionCycle.UnifiedBillingCycle != null)
                    {
                        if (subscriptionCycle.UnifiedBillingCycle.endDate < DateUtils.DateTimeToUtcUnixTimestampMilliseconds(renewDetails.EndDate.Value) &&
                            subscriptionCycle.UnifiedBillingCycle.endDate < DateUtils.DateTimeToUtcUnixTimestampMilliseconds(DateTime.UtcNow))
                        {
                            // update unified billing by endDate or paymentGatewatId                  
                            bool setResult = UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(renewDetails.DomainId, renewDetails.MaxVLCOfSelectedUsageModule, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(renewDetails.EndDate.Value));
                        }
                        else
                        {
                            renewDetails.EndDate = DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate);
                        }

                        log.DebugFormat("New end-date was updated according to UnifiedBillingCycle. EndDate={0}", renewDetails.EndDate);
                    }

                    if (unifiedProcess.isNew)
                    {
                        //todo create unified
                        HandleRenewUnifiedSubscriptionPending(renewDetails.GroupId, (int)renewDetails.DomainId, unifiedProcess.EndDate, paymentGateway.RenewalIntervalMinutes, unifiedProcess.Id);
                    }
                }
            }
            else
            {
                // end wasn't retuned - get next end date from MPP
                renewDetails.EndDate = Utils.GetEndDateTime(renewDetails.EndDate.Value, renewDetails.MaxVLCOfSelectedUsageModule);

                if (new Duration(renewDetails.MaxVLCOfSelectedUsageModule).IsMonthlyLifeCycle() && renewDetails.EndDate.Value.Day >= 28 && renewDetails.StartDate.Value.Day > renewDetails.EndDate.Value.Day)
                {
                    int newDay = Math.Min(DateTime.DaysInMonth(renewDetails.EndDate.Value.Year, renewDetails.EndDate.Value.Month), renewDetails.StartDate.Value.Day);
                    renewDetails.EndDate = renewDetails.EndDate.Value.AddDays(newDay - renewDetails.EndDate.Value.Day);
                }

                log.DebugFormat("New end-date was updated according to MPP. EndDate={0}", renewDetails.EndDate);
            }

            try
            {
                if (!UpdateMPPRenewData(cas, subscription, renewDetails, transactionResponse.TransactionID, renewDetails.EndDate, unifiedProcess?.Id > 0 ? unifiedProcess.Id : 0))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to update MPP renew data", ex);
                return true;
            }

            // update billing_transactions subscriptions_purchased reference  
            if (transactionResponse.TransactionID > 0)
            {
                ApiDAL.Update_PurchaseIDInBillingTransactions(transactionResponse.TransactionID, renewDetails.PurchaseId);
            }
            else
            {
                log.Error("Error while trying update billing_transactions subscriptions_purchased reference");
            }

            if (subscriptionCycle == null || subscriptionCycle.UnifiedBillingCycle == null)
            {
                // enqueue renew transaction
                RenewTransactionsQueue queue = new RenewTransactionsQueue();
                DateTime nextRenewalDate = renewDetails.EndDate.Value.AddMinutes(-5);

                if (paymentGateway != null)
                {
                    nextRenewalDate = renewDetails.EndDate.Value.AddMinutes(paymentGateway.RenewalStartMinutes);
                }
                var data = new RenewTransactionData(renewDetails, DateUtils.DateTimeToUtcUnixTimestampSeconds(renewDetails.EndDate.Value), nextRenewalDate);
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, renewDetails.GroupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                    return true;
                }
                else
                {
                    PurchaseManager.SendRenewalReminder(data, renewDetails.DomainId);
                    log.DebugFormat("New task created (upon renew success response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }
            }

            // PS message 
            var dicData = new Dictionary<string, object>()
            {
                {"BillingTransactionID", transactionResponse.TransactionID},
                {"SiteGUID", renewDetails.UserId},
                {"PaymentNumber", renewDetails.PaymentNumber},
                {"TotalPaymentsNumber", renewDetails.RecurringData.TotalNumOfRenews},
                {"CustomData", renewDetails.CustomData},
                {"Price", renewDetails.Price},
                {"PurchaseID", renewDetails.PurchaseId},
                {"SubscriptionCode", subscription.m_SubscriptionCode}
            };

            cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, dicData, renewDetails.UserId, string.Empty, string.Empty);

            log.DebugFormat("Successfully renewed. productId: {0}, price: {1}, currency: {2}, userID: {3}, billingTransactionId: {4}",
                            renewDetails.ProductId, renewDetails.Price, renewDetails.Currency, renewDetails.UserId, transactionResponse.TransactionID);

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
            if (Math.Abs(TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate) - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling GiftCardReminder task." +
                    "Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate), logString);
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
                    subscription = Core.Pricing.Module.Instance.GetSubscriptionData(groupId, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
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
            string customData, int maxVLCOfSelectedUsageModule, long billingTransitionId, string userIp)
        {

            // end wasn't retuned - get next end date from MPP
            endDate = Utils.GetEndDateTime(endDate, maxVLCOfSelectedUsageModule);
            log.DebugFormat("New end-date was updated according to MPP. EndDate={0}", endDate);

            // update MPP renew data
            try
            {
                ConditionalAccessDAL.Update_MPPRenewalData(purchaseId, true, endDate, 0, "CA_CONNECTION_STRING", siteguid, null, billingTransitionId);
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

            string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, householdId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key on Renew key = {0}", invalidationKey);
            }

            // update billing_transactions subscriptions_purchased reference  
            if (billingTransitionId > 0 && !ApiDAL.Update_PurchaseIDInBillingTransactions(billingTransitionId, purchaseId))
            {
                log.Error("Error while trying update billing_transactions subscriptions_purchased reference");
            }

            long endDateUnix = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate);
            DateTime nextRenewalDate = endDate.AddMinutes(0);

            var queue = new RenewTransactionsQueue();
            var data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, endDateUnix, nextRenewalDate);
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

                cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, dicData, siteguid, string.Empty, userIp);
            }

            log.DebugFormat("Successfully renewed. productId: {0}, price: {1}, currency: {2}, userID: {3}, billingTransactionId: {4}",
                productId,                          // {0}
                price,                              // {1}
                currency,                           // {2}
                siteguid,                           // {3}
                billingTransitionId);               // {4}

            return true;
        }

        protected internal static bool HandleDummySubsciptionRenewal(BaseConditionalAccess cas, int groupId, RenewDetails renewDetails, string billingGuid, string logString, string userIp, XmlNode theRequest)
        {
            bool saveHistory = XmlUtils.IsNodeExists(ref theRequest, BaseConditionalAccess.HISTORY);
            string udid = XmlUtils.GetSafeValue(BaseConditionalAccess.DEVICE_NAME, ref theRequest);
            int oldRecurringNumber = 0, numOfPayments = 0;

            if (!int.TryParse(XmlUtils.GetSafeValue(BaseConditionalAccess.RECURRING_NUMBER, ref theRequest), out oldRecurringNumber))
            {
                // Subscription ended
                log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse recurringNumber,  data: {0}", logString);
                cas.WriteToUserLog(renewDetails.UserId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                                                                      renewDetails.ProductId, numOfPayments, oldRecurringNumber, numOfPayments, billingGuid));
                return false;
            }

            if (!int.TryParse(XmlUtils.GetSafeParValue("//p", "o", ref theRequest), out numOfPayments))
            {
                // Subscription ended
                log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse //p o,  data: {0}", logString);
                cas.WriteToUserLog(renewDetails.UserId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                                                                      renewDetails.ProductId, numOfPayments, oldRecurringNumber, numOfPayments, billingGuid));
                return false;
            }

            int newRecurringNumber = Utils.CalcPaymentNumber(numOfPayments, oldRecurringNumber, false);
            if (numOfPayments > 0 && newRecurringNumber > numOfPayments)
            {
                // Subscription ended
                log.ErrorFormat("Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}", numOfPayments, newRecurringNumber, numOfPayments);
                cas.WriteToUserLog(renewDetails.UserId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
                                                                      renewDetails.ProductId, numOfPayments, newRecurringNumber, numOfPayments, billingGuid));
                return true;
            }

            // calculate payment (recurring) number
            newRecurringNumber++;

            string price = XmlUtils.GetSafeValue(BaseConditionalAccess.PRICE, ref theRequest);
            string mumlc = XmlUtils.GetSafeValue(MAX_USAGE_MODULE, ref theRequest);

            Subscription subscription = null;
            string pricingUsername = string.Empty, pricingPassword = string.Empty;
            Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref pricingUsername, ref pricingPassword);
            Subscription[] subscriptions = Utils.GetSubscriptionsDataWithCaching(new List<long>(1) { renewDetails.ProductId }, groupId);
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
                renewDetails.CustomData = renewDetails.CustomData.Replace(string.Format("<recurringnumber>{0}</recurringnumber>", oldRecurringNumber), string.Format("<recurringnumber>{0}</recurringnumber>", newRecurringNumber));
            }

            long billingTransactionID = 0;

            if (saveHistory)
            {
                // purchase
                BillingResponse billingResponse = new BillingResponse
                {
                    m_oStatus = BillingResponseStatus.UnKnown
                };

                billingResponse = cas.HandleCCChargeUser(renewDetails.UserId, double.Parse(price), renewDetails.PreviousPurchaseCurrencyCode, userIp, renewDetails.CustomData,
                                                         newRecurringNumber, numOfPayments, string.Empty, string.Empty, string.Empty, true, false);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success || !long.TryParse(billingResponse.m_sRecieptCode, out billingTransactionID))
                {
                    return false;
                }

            }

            var endDate = renewDetails.EndDate.Value;
            return HandleRenewGrantedSubscription(cas, groupId, renewDetails.UserId, renewDetails.PurchaseId, billingGuid, renewDetails.ProductId, ref endDate, renewDetails.DomainId,
               double.Parse(price), renewDetails.PreviousPurchaseCurrencyCode, newRecurringNumber, numOfPayments, subscription, theRequest.InnerXml, int.Parse(mumlc), billingTransactionID, userIp);
        }

        public static bool RenewUnifiedTransaction(BaseConditionalAccess cas, int groupId, long householdId, long nextEndDate, long processId, ref List<long> purchasesIds)
        {
            // log request
            string logString = string.Format("RenewUnifiedTransaction: householdId {0}, processId {1}, endDateLong {2}", householdId, processId, nextEndDate);
            log.DebugFormat("Starting renewal one transaction process. data: {0}", logString);
            string customData = string.Empty;
            string userIp = "1.1.1.1";

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
            if (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
            {
                if (!APILogic.Api.Managers.RolesPermissionsManager.Instance.IsPermittedPermission(groupId, domain.m_masterGUIDs[0].ToString(), RolePermissions.RENEW_SUBSCRIPTION))
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
            DateTime? processEndDate = null, processCreateDate = null;

            if (UpdateProcessDetailsForRenewal(processId, ref paymentgatewayId, ref processState, ref processEndDate, out processCreateDate))
            {
                long processEndDateUnix = DateUtils.DateTimeToUtcUnixTimestampMilliseconds(processEndDate.Value);
                // validate that this is the right message                              
                if (Math.Abs(processEndDateUnix - nextEndDate) > 60000)
                {
                    // subscription purchase wasn't found
                    log.ErrorFormat("GetProcessDetails if end date not equal - canceling unified renew task. processId: {0}, nextEndDate: {1}, data: {2}.",
                                    processId, processEndDateUnix, logString);
                    ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null, null);
                    return true;
                }
            }
            else
            {
                log.DebugFormat("No data return from DB for processId {0}", processId);
                return true;
            }

            HouseholdPaymentGateway householdPaymentGateway = DAL.BillingDAL.GetHouseholdPaymentGateway(groupId, paymentgatewayId, householdId);
            if (householdPaymentGateway == null || householdPaymentGateway.PaymentGatewayId != paymentgatewayId)
            {
                bool changeUnifiedStatus = ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null, null);
                log.Debug("RenewUnifiedTransaction fail Household not set to paymentGateway");
                return true;
            }

            if (householdPaymentGateway.Status == PaymentGatewayStatus.Suspend && householdPaymentGateway.SuspendSettings != null && householdPaymentGateway.SuspendSettings.StopRenew)
            {
                bool changeUnifiedStatus = ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null, null);
                log.Debug($"RenewUnifiedTransaction Household paymentGateway Suspend - Stop Renew - changeUnifiedStatus:{changeUnifiedStatus}");
                return true;
            }

            #region Get subscriptions purchase and renew details

            // get subscription purchase 
            DataTable subscriptionPurchaseDt = ConditionalAccessDAL.Set_SubscriptionPurchaseUnifiedForRenewal(groupId, householdId, processId);

            // validate subscription received
            if (subscriptionPurchaseDt == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("problem getting the subscription purchase. householdId : {0}, data: {1}", householdId, logString);
                return false; // retry
            }

            var renewDetailsListResponse = GetRenewDetailsList(subscriptionPurchaseDt, groupId, cas, out Dictionary<long, Subscription> subscriptionsMap, logString);
            if (!renewDetailsListResponse.HasObjects())// all resDetails were addon with none relevant base
            {
                log.Debug($"RenewUnifiedTransaction fail due to no addon subscriptions for householdid={householdId} and paymentgatewayid={paymentgatewayId}.");
                return true;
            }
            var renewDetailsList = renewDetailsListResponse.Objects;
            purchasesIds = renewDetailsList.Select(s => s.PurchaseId).ToList();

            // get unified billing cycle for this household 
            var subscriptionCycle = Utils.GetSubscriptionCycle(groupId, (int)householdId, renewDetailsList[0].MaxVLCOfSelectedUsageModule);
            if (!subscriptionCycle.HasCycle || subscriptionCycle.UnifiedBillingCycle == null)
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no groupBillingCycle define in CB for groupId={0}", groupId);
                return true;
            }

            if (subscriptionCycle.PaymentGatewayId == 0)
            {
                subscriptionCycle.PaymentGatewayId = paymentgatewayId;
            }

            #endregion

            log.DebugFormat("subscription purchases found and validated. data: {0}", logString);

            #region check addon subscriptions

            var removeSubscriptionCodes = new List<long>();
            var subscriptionsMapByType = subscriptionsMap.Values.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());
            if (subscriptionsMapByType.ContainsKey(SubscriptionType.AddOn))
            {
                var baseSubscriptions = subscriptionsMapByType.ContainsKey(SubscriptionType.Base) ? subscriptionsMapByType[SubscriptionType.Base] : null;

                foreach (Subscription subscription in subscriptionsMapByType[SubscriptionType.AddOn])
                {
                    if (subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
                    {
                        var purchaseAddOnStatus = Utils.CanPurchaseAddOn(groupId, householdId, subscription, baseSubscriptions, processEndDate);
                        if (purchaseAddOnStatus.IsOkStatusCode()) { continue; }

                        // check mabye this add on have base subscription in this unified billing cycle 
                        bool canPurchaseAddOn = false;

                        // get all setsIds for this addon 
                        var addOnSetIds = subscription.GetSubscriptionSetIdsToPriority();

                        // check if one of the subscription are base in this unified cycle 
                        if (baseSubscriptions != null)
                        {
                            foreach (Subscription baseSubscription in baseSubscriptions)
                            {
                                var baseSetIds = baseSubscription.GetSubscriptionSetIdsToPriority();
                                if (baseSetIds.Count(x => addOnSetIds.ContainsKey(x.Key)) > 0)
                                {
                                    canPurchaseAddOn = true;
                                    break;
                                }
                            }
                        }

                        if (!canPurchaseAddOn)
                        {
                            // change is recurring to false and call event handle- this renew subscription failed!
                            var subscriptionCode = int.Parse(subscription.m_SubscriptionCode);
                            RenewDetails rsDetail = renewDetailsList.FirstOrDefault(x => x.ProductId == subscriptionCode);

                            if (HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, rsDetail, 0, "AddOn with no BaseSubscription valid", string.Empty, nextEndDate))
                            {
                                // save all SubscriptionCode to remove from subscription list 
                                removeSubscriptionCodes.Add(subscriptionCode);

                                // remove this renewDetails (it's an AddOn)
                                renewDetailsList.Remove(rsDetail);
                            }

                            log.DebugFormat("stoping renew add-on subscription id: {0}, code = {1}, message = {2}",
                                            subscription.m_SubscriptionCode, purchaseAddOnStatus.Code, purchaseAddOnStatus.Message);
                        }
                    }
                }
            }

            // all resDetails were addon with none relevant base
            if (renewDetailsList.Count == 0)
            {
                log.DebugFormat("stop renew process due to no subs to renew");
                return true;
            }

            // remove all not relevant subscriptions
            if (removeSubscriptionCodes.Count > 0)
            {
                foreach (var subCodeToRemove in removeSubscriptionCodes)
                {
                    subscriptionsMap.Remove(subCodeToRemove);
                }
            }

            #endregion

            Utils.GetMultiSubscriptionUsageModule(renewDetailsList, userIp, subscriptionsMap, cas, ref subscriptionCycle, (int)householdId, groupId);

            // call billing process renewal
            TransactResult transactionResponse = null;

            // call to each group of subscription that have the same : paymentmethode id + currency                
            var renewDetailsMapByPaymentMethodIdAndCurrency = renewDetailsList.GroupBy(r => string.Format("{0}_{1}", r.PaymentMethodId, r.Currency)).ToDictionary(g => g.Key, g => g.ToList());

            // save dictionary for the resulte from adapter - handle it after 
            List<string> successTransactions = new List<string>();
            List<string> pendingTransactions = new List<string>();
            PaymentGateway paymentGateway = DAL.BillingDAL.GetPaymentGateway(groupId, paymentgatewayId);

            foreach (var mappedRenewDetails in renewDetailsMapByPaymentMethodIdAndCurrency)
            {
                List<RenewDetails> groupedRenewDetailsList = mappedRenewDetails.Value;
                double totalPrice = groupedRenewDetailsList.Sum(x => x.Price);
                string currency = groupedRenewDetailsList.FirstOrDefault().Currency;
                int paymentMethodId = groupedRenewDetailsList.FirstOrDefault().PaymentMethodId;

                try
                {
                    transactionResponse = Billing.Module.ProcessUnifiedRenewal(groupId, householdId, totalPrice, currency, paymentgatewayId, paymentMethodId, userIp, ref groupedRenewDetailsList, ref paymentGateway, processId);
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
                            foreach (RenewDetails renewDetails in groupedRenewDetailsList)
                            {
                                Subscription subscription = subscriptionsMap[renewDetails.ProductId];
                                HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, renewDetails, 0, transactionResponse.Status.Message, string.Empty, nextEndDate);
                            }
                            continue;
                        }
                    }

                    switch (transactionResponse.State)
                    {
                        case eTransactionState.OK:
                            {
                                HandleRenewUnifiedSubscriptionSuccess(cas, groupId, householdId, customData, ref subscriptionCycle, currency, subscriptionsMap, groupedRenewDetailsList, paymentGateway,
                                                                      processCreateDate, processId);
                                successTransactions.Add(mappedRenewDetails.Key);
                            }
                            break;
                        case eTransactionState.Pending:
                            {
                                // get configuration value 
                                int PendingThresholdDays = ApplicationConfiguration.Current.PendingThresholdDays.Value;

                                if (PendingThresholdDays == 0)
                                {
                                    PendingThresholdDays = PENDING_THRESHOLD_DAYS;
                                }

                                if (new DateTime(1970, 1, 1).AddMilliseconds(nextEndDate).AddDays(PendingThresholdDays) < DateTime.UtcNow)
                                {
                                    // renew subscription failed - stop renew attempts!
                                    foreach (RenewDetails renewUnifiedData in groupedRenewDetailsList)
                                    {
                                        Subscription subscription = subscriptionsMap[renewUnifiedData.ProductId];
                                        HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, renewUnifiedData, transactionResponse.FailReasonCode, null, string.Empty, nextEndDate);
                                    }
                                }
                                else
                                {
                                    pendingTransactions.Add(mappedRenewDetails.Key);
                                }
                            }
                            break;
                        case eTransactionState.Failed:
                            {
                                log.DebugFormat($"Transaction state is failed");

                                // renew subscription failed - stop renew attempts!
                                foreach (RenewDetails renewUnifiedData in groupedRenewDetailsList)
                                {
                                    Subscription subscription = subscriptionsMap[renewUnifiedData.ProductId];
                                    HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, renewUnifiedData, transactionResponse.FailReasonCode, null, string.Empty, nextEndDate);
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
                    successTransactionsEndDate = DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate);
                    bool setResult = UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(householdId, subscriptionCycle.SubscriptionLifeCycle.GetTvmDuration(), subscriptionCycle.UnifiedBillingCycle.endDate);
                }

                if (pendingTransactions.Count > 0)
                {
                    pendingTransactionsEndDate = DateUtils.UtcUnixTimestampMillisecondsToDateTime(nextEndDate);
                }

                UpdateNextUnifiedCycle(groupId, householdId, paymentGateway, subscriptionCycle.SubscriptionLifeCycle.GetTvmDuration(), processId, processState, successTransactions, pendingTransactions, renewDetailsMapByPaymentMethodIdAndCurrency, successTransactionsEndDate, pendingTransactionsEndDate);
            }
            else
            {
                bool changeUnifiedStatus = ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null, subscriptionCycle.SubscriptionLifeCycle.GetTvmDuration());
            }

            return true; // no need to retry renew 
        }


        private static void UpdateNextUnifiedCycle(int groupId, long householdId, PaymentGateway paymentGateway, long cycle, long processId, ProcessUnifiedState processState,
            List<string> successTransactions, List<string> pendingTransactions, Dictionary<string, List<RenewDetails>> renewUnifiedDict, DateTime? successTransactionsEndDate, DateTime? pendingTransactionsEndDate)
        {
            // insert right messages + update db

            try
            {
                if (successTransactions.Count > 0)
                {
                    string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, householdId);
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
                                UpdateSuccessTransactions(groupId, householdId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value, true, cycle);

                                // update the pandings 
                                if (pendingTransactions.Count > 0) // some are pending this renew process
                                {
                                    // get from db process for this with Pending State if exsits
                                    bool isNew = false;
                                    long pendingProcessId = Utils.GetUnifiedProcessId(groupId, paymentGateway.ID, pendingTransactionsEndDate.Value, householdId, cycle, out isNew, ProcessUnifiedState.Pending);
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
                                ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, (int)ProcessUnifiedState.Pending, cycle);

                                // insert message to queue
                                HandleRenewUnifiedSubscriptionPending(groupId, householdId, pendingTransactionsEndDate.Value, paymentGateway.RenewalIntervalMinutes, processId);
                            }
                        }
                        break;
                    case ProcessUnifiedState.Pending:
                        {
                            if (pendingTransactions.Count > 0) // some still in pending status 
                            {
                                ConditionalAccessDAL.UpdateUnifiedProcess(processId, null, null, cycle);

                                // insert message to queue
                                HandleRenewUnifiedSubscriptionPending(groupId, householdId, pendingTransactionsEndDate.Value, paymentGateway.RenewalIntervalMinutes, processId);

                                // update the pandings success
                                if (successTransactions.Count > 0) // some are success this renew process
                                {
                                    bool isNew = false;
                                    long renewProcessId = Utils.GetUnifiedProcessId(groupId, paymentGateway.ID, successTransactionsEndDate.Value, householdId, cycle, out isNew, ProcessUnifiedState.Renew);
                                    if (renewProcessId > 0)
                                    {
                                        // update subscription purchases table with process id
                                        List<int> purchaseIds = (renewUnifiedDict.Where(x => successTransactions.Contains(x.Key)).SelectMany(x => x.Value).ToList()).Select(y => (int)y.PurchaseId).ToList();
                                        ConditionalAccessDAL.UpdateMPPRenewalProcessId(purchaseIds, renewProcessId);

                                        if (isNew)
                                        {
                                            UpdateSuccessTransactions(groupId, householdId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value, false, cycle);
                                        }
                                    }
                                }
                            }
                            else // all success
                            {
                                long successProcessId = 0;
                                DataTable dt = ConditionalAccessDAL.GetUnifiedProcessId(groupId, paymentGateway.ID, successTransactionsEndDate.Value, householdId, cycle, (int)ProcessUnifiedState.Renew);
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
                                    UpdateSuccessTransactions(groupId, householdId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(successTransactionsEndDate.Value), paymentGateway, processId, successTransactionsEndDate.Value, true, cycle);
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

        private static bool UpdateSuccessTransactions(int groupId, long householdId, long endDate, PaymentGateway paymentGateway, long processId, DateTime endDatedt, bool updateUnifiedProcess = true, long? cycle = null)
        {
            bool saved = true;
            DateTime nextRenewalDate = endDatedt.AddMinutes(paymentGateway.RenewalStartMinutes);

            // insert message to queue
            Utils.RenewUnifiedTransactionMessageInQueue(groupId, householdId, endDate, nextRenewalDate, processId);

            if (updateUnifiedProcess)
            {
                // update date for next process
                saved = ConditionalAccessDAL.UpdateUnifiedProcess(processId, endDatedt, (int)ProcessUnifiedState.Renew, cycle);
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

        private static bool HandleRenewUnifiedSubscriptionSuccess(BaseConditionalAccess cas, int groupId, long householdId, string customData, ref SubscriptionCycle subscriptionCycle,
                                                                  string currency, Dictionary<long, Subscription> subscriptions, List<RenewDetails> renewUnifiedDetailsList, PaymentGateway paymentGateway,
                                                                  DateTime? processCreateDate, long processId)
        {
            DateTime? endDate = null;
            Subscription subscription = null;
            int purchaseDay = processCreateDate.HasValue ? processCreateDate.Value.Day : 0;

            foreach (var renewUnifiedDetails in renewUnifiedDetailsList)
            {
                if (!endDate.HasValue)
                {
                    if (renewUnifiedDetails.SubscriptionStatus == SubscriptionPurchaseStatus.Suspended && renewUnifiedDetails.EndDate.Value < DateTime.UtcNow)
                    {
                        endDate = Utils.GetEndDateTime(subscriptionCycle.SubscriptionLifeCycle, DateTime.UtcNow);
                    }
                    else
                    {
                        endDate = Utils.GetEndDateTime(subscriptionCycle.SubscriptionLifeCycle, renewUnifiedDetails.EndDate.Value);
                        if (subscriptionCycle.SubscriptionLifeCycle.Unit == DurationUnit.Months && endDate.Value.Day >= 28 && purchaseDay > endDate.Value.Day)
                        {
                            int newDay = Math.Min(DateTime.DaysInMonth(endDate.Value.Year, endDate.Value.Month), purchaseDay);
                            endDate = endDate.Value.AddDays(newDay - endDate.Value.Day);
                        }
                    }

                    DateTime ubcDate = DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate);

                    if (ubcDate < endDate)
                    {
                        log.DebugFormat("going to update UnifiedBillingCycle. current date = {0}, new Date = {1}", subscriptionCycle.UnifiedBillingCycle.endDate.ToString(), endDate.Value);

                        // update unified billing cycle for domian with next end date
                        long nextEndDate = DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate.Value);
                        Utils.HandleDomainUnifiedBillingCycle(groupId, householdId, renewUnifiedDetails.MaxVLCOfSelectedUsageModule, nextEndDate);
                        subscriptionCycle.UnifiedBillingCycle.endDate = nextEndDate;
                    }
                }

                subscription = subscriptions[renewUnifiedDetails.ProductId];

                // update MPP renew data
                try
                {
                    renewUnifiedDetails.StartDate = processCreateDate;
                    if (!UpdateMPPRenewData(cas, subscription, renewUnifiedDetails, renewUnifiedDetails.BillingTransactionId, endDate, processId))
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error while trying to update MPP renew data", ex);
                    return true;
                }

                // message for PS use
                Dictionary<string, object> psMessage = new Dictionary<string, object>()
                {
                    {"BillingTransactionID", renewUnifiedDetails.BillingTransactionId},
                    {"SiteGUID", renewUnifiedDetails.UserId},
                    {"PaymentNumber", renewUnifiedDetails.PaymentNumber},
                    {"TotalPaymentsNumber", renewUnifiedDetails.RecurringData.TotalNumOfRenews},
                    {"CustomData", renewUnifiedDetails.CustomData},
                    {"Price", renewUnifiedDetails.Price},
                    {"PurchaseID", renewUnifiedDetails.PurchaseId},
                    {"SubscriptionCode", subscription.m_SubscriptionCode}
                };

                cas.EnqueueEventRecord(NotifiedAction.ChargedSubscriptionRenewal, psMessage, renewUnifiedDetails.UserId, string.Empty, string.Empty);
            }

            return true;
        }

        private static bool UpdateMPPRenewData(BaseConditionalAccess cas, Subscription subscription, RenewDetails renewDetails, long transactionId, DateTime? renewDetailsEndDate, long unifiedProcessId)
        {
            // grant entitlement
            bool isUsageModuleExists = subscription.m_oUsageModule != null;
            SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(renewDetails.GroupId)
            {
                purchaseId = (int)renewDetails.PurchaseId,
                siteGuid = renewDetails.UserId,
                productId = renewDetails.ProductId.ToString(), //subscription.m_SubscriptionCode,
                status = SubscriptionPurchaseStatus.OK,
                price = renewDetails.Price,
                currency = renewDetails.Currency,
                customData = renewDetails.CustomData,
                billingGuid = renewDetails.BillingGuid,
                isRecurring = subscription.m_bIsRecurring,
                endDate = renewDetailsEndDate,
                houseHoldId = renewDetails.DomainId,
                billingTransactionId = transactionId,
                couponCode = renewDetails.RecurringData.CouponCode,
                usageModuleExists = isUsageModuleExists,
                viewLifeCycle = isUsageModuleExists ? subscription.m_oUsageModule.m_tsViewLifeCycle : 0,
                maxNumberOfViews = isUsageModuleExists ? subscription.m_oUsageModule.m_nMaxNumberOfViews : 0,
                previewModuleId = subscription.m_oPreviewModule != null ? subscription.m_oPreviewModule.m_nID : 0,
                processPurchasesId = unifiedProcessId,
                entitlementDate = renewDetails.StartDate
                //country = country,
                //deviceName = deviceName,
                //isEntitledToPreviewModule = isEntitledToPreviewModule,
            };

            bool success = subscriptionPurchase.Update();

            if (!success)
            {
                log.Error("Error while trying to renew subscription");
                return false;
            }
            else
            {
                log.Debug("Subscription was renewed");

                // update compensation use
                if (renewDetails.RecurringData.Compensation != null)
                {
                    renewDetails.RecurringData.Compensation.Renewals++;
                    if (!ConditionalAccessDAL.UpdateSubscriptionCompensationUse(renewDetails, transactionId))
                    {
                        log.ErrorFormat("Failed to update subscription compensation use. compensationId = {0}, billingTransactionId = {1}, renewalNumber = {2}",
                                         renewDetails.RecurringData.Compensation.Id, transactionId, renewDetails.RecurringData.Compensation.Renewals);
                    }

                    if (renewDetails.RecurringData.Compensation.TotalRenewals > 0 
                        && renewDetails.RecurringData.Compensation.Renewals == renewDetails.RecurringData.Compensation.TotalRenewals)
                    {
                        renewDetails.RecurringData.Compensation = null;
                    }
                }

                if (renewDetails.IsUseCouponRemainder ||
                    (!renewDetails.RecurringData.IsCouponHasEndlessRecurring && renewDetails.RecurringData.LeftCouponRecurring == 0 && !string.IsNullOrEmpty(renewDetails.RecurringData.CouponCode)))
                {
                    renewDetails.RecurringData.CouponRemainder = 0;
                    renewDetails.RecurringData.CouponCode = string.Empty;
                    renewDetails.RecurringData.LeftCouponRecurring = 0;
                    renewDetails.RecurringData.IsCouponGiftCard = false;
                    renewDetails.RecurringData.IsCouponHasEndlessRecurring = false;
                }

                renewDetails.RecurringData.TotalNumOfRenews++;

                if (renewDetails.RecurringData.LeftCouponRecurring > 0)
                {
                    renewDetails.RecurringData.LeftCouponRecurring--;
                }

                if (renewDetails.RecurringData.CampaignDetails != null)
                {
                    if (renewDetails.RecurringData.CampaignDetails.IsUseRemainder)
                    {
                        renewDetails.RecurringData.CampaignDetails = null;
                    }

                    if (renewDetails.RecurringData.CampaignDetails.LeftRecurring > 0)
                    {
                        renewDetails.RecurringData.CampaignDetails.LeftRecurring--;

                        if (renewDetails.RecurringData.CampaignDetails.LeftRecurring == 0 && renewDetails.RecurringData.CampaignDetails.Remainder == 0)
                        {
                            renewDetails.RecurringData.CampaignDetails = null;
                        }
                    }
                }

                ConditionalAccessDAL.SaveRecurringRenewDetails(renewDetails.RecurringData, renewDetails.PurchaseId);
            }

            cas.WriteToUserLog(renewDetails.UserId, string.Format("Successfully renewed. Product ID: {0}, price: {1}, currency: {2}, purchase ID: {3}, Billing Transition ID: {4}",
                                                                  renewDetails.ProductId, renewDetails.Price, renewDetails.Currency, renewDetails.PurchaseId, transactionId));

            return true;
        }

        public static bool HandleRenewUnifiedSubscriptionPending(int groupId, long householdId, DateTime endDate, int renewalIntervalMinutes, long processID)
        {
            try
            {
                DateTime nextRenewalDate = DateTime.UtcNow.AddMinutes(renewalIntervalMinutes);

                // enqueue unified renew transaction
                Utils.RenewUnifiedTransactionMessageInQueue(groupId, householdId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate), nextRenewalDate, processID);
            }
            catch (Exception ex)
            {
                log.Error("fail HandleRenewUnifiedSubscriptionPending ex={0}", ex);
            }
            return true;
        }

        private static bool HandleRenewUnifiedSubscriptionFailed(BaseConditionalAccess cas, int groupId, int paymentgatewayId, long householdId, Subscription subscription,
            RenewDetails renewDetails, int failReasonCode, string billingSettingError = null, string billingGuid = null, long endDateUnix = 0)
        {
            string logString = string.Empty;

            logString = string.Format("Unified Purchase request (fail): householdId:{0}, siteguid {1}, purchaseId {2}, billingGuid {3}",
                                      householdId, renewDetails.UserId, renewDetails.PurchaseId, renewDetails.BillingGuid);
            renewDetails.GroupId = groupId;
            renewDetails.DomainId = householdId;
            renewDetails.BillingGuid = billingGuid;

            return HandleRenewSubscriptionFailed(cas, renewDetails, logString, subscription, failReasonCode, billingSettingError, endDateUnix / 1000);
        }

        public static bool HandleResumeDomainSubscription(int groupId, long householdId, string siteguid, long purchaseId, string billingGuid, DateTime endDate)
        {
            try
            {
                log.DebugFormat("HandleResumeDomainSubscription. groupId: {0}, householdId: {1}, siteguid: {2}, purchaseId: {3}, billingGuid: {4}", groupId, householdId, siteguid, purchaseId, billingGuid);

                bool enqueueSuccessful = true;

                DateTime nextRenewalDate = DateTime.UtcNow;
                long endDateUnix = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate);

                var data = new RenewTransactionData(groupId, siteguid, purchaseId, billingGuid, endDateUnix, nextRenewalDate);
                var queue = new RenewTransactionsQueue();
                enqueueSuccessful &= queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction for resume domain {0}", data);
                    return false;
                }
                else
                {
                    log.DebugFormat("New task created (upon renew pending response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                }

                if (enqueueSuccessful)
                {
                    PurchaseManager.SendRenewalReminder(data, householdId);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public static EntitlementRenewalResponse GetEntitlementNextRenewal(BaseConditionalAccess cas, int groupId, long householdId, long purchaseId, long userId)
        {
            EntitlementRenewalResponse response = new EntitlementRenewalResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // get subscription purchase 
            DataRow subscriptionRenealDataRow = DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseNextRenewal(groupId, householdId, purchaseId);

            // validate subscription received
            if (subscriptionRenealDataRow == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("GetEntitlementNextRenewal: failed subscription purchase. PurchaseId: {0}, householdId: {1}", purchaseId, householdId);
                return response;
            }

            var renewDetails = new RenewDetails()
            {
                ProductId = ODBCWrapper.Utils.ExtractInteger(subscriptionRenealDataRow, "SUBSCRIPTION_CODE"), // AKA subscription ID/CODE
                PurchaseId = purchaseId,
                BillingGuid = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "billing_Guid"),
                //BillingTransactionId
                //ExternalTransactionId
                UserId = "",
                DomainId = householdId,
                ShouldSwitchToMasterUser = false,
                GroupId = groupId,
                Currency = "n/a",
                // price
                // PaymentMethodId
                PaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRenealDataRow, "PAYMENT_NUMBER"),
                NumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRenealDataRow, "number_of_payments"),
                CustomData = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "CUSTOMDATA"), // AKA subscription ID/CODE
                EndDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionRenealDataRow, "END_DATE")
            };

            if (!TryGetSubscription(groupId, (int)renewDetails.ProductId, out Subscription subscription, string.Format("GetEntitlementNextRenewal: Error while trying to fetch subscription data. PurchaseId: {0}, householdId: {1}", purchaseId, householdId)))
            {
                return response;
            }

            if (!SetCustomDataForRenewDetails(groupId, renewDetails, out bool isDummy, out XmlNode theRequest))
            {
                return response;
            }

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
                        SubscriptionId = renewDetails.ProductId,
                        PurchaseId = purchaseId,
                        Date = renewDetails.EndDate.Value,
                        GroupId = groupId,
                        Id = purchaseId,
                        UserId = userId
                    },
                    Status = ApiObjects.Response.Status.Ok
                };

                return response;
            }

            var renewDetailsRow = ConditionalAccessDAL.Get_RenewDetails(groupId, purchaseId, renewDetails.BillingGuid);
            if (renewDetailsRow != null)
            {
                renewDetails.PaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "PAYMENT_NUMBER");
                renewDetails.NumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "number_of_payments");
            }

            renewDetails.RecurringData = ConditionalAccessDAL.GetRecurringRenewDetails(purchaseId);
            if (renewDetails.RecurringData == null)
            {
                var couponCode = ODBCWrapper.Utils.ExtractString(subscriptionRenealDataRow, "coupon_code");
                renewDetails.RecurringData = EntitlementManager.InitializeRecurringRenewDetails(groupId, subscriptionRenealDataRow, renewDetails.PurchaseId, subscription, renewDetails.BillingGuid, couponCode);
            }

            renewDetails.PaymentNumber = Utils.CalcPaymentNumber(renewDetails.NumOfPayments, renewDetails.PaymentNumber, renewDetails.RecurringData.IsPurchasedWithPreviewModule);

            if (renewDetails.NumOfPayments > 0 && renewDetails.PaymentNumber > renewDetails.NumOfPayments)
            {
                // Subscription ended
                log.ErrorFormat("GetEntitlementNextRenewal: Subscription ended. numOfPayments={0}, paymentNumber={1}", renewDetails.NumOfPayments, renewDetails.PaymentNumber);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            // calculate payment number
            renewDetails.PaymentNumber++;

            bool ignoreUnifiedBillingCycle = false;

            // Check if this is a renew via INAPP PURCHASE
            if (!string.IsNullOrEmpty(renewDetails.BillingGuid))
            {
                PaymentDetails pd = null;
                ApiObjects.Response.Status statusVerifications = Billing.Module.GetPaymentGatewayVerificationStatus(groupId, renewDetails.BillingGuid, ref pd);
                ignoreUnifiedBillingCycle = statusVerifications.Code != (int)eResponseStatus.OK || pd == null || pd.PaymentGatewayId == 0;
            }

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

            // get MPP
            int recPeriods = 0;
            bool isMPPRecurringInfinitely = false;
            UnifiedBillingCycle unifiedBillingCycle = null;
            string userIp = "";

            if (!cas.GetMultiSubscriptionUsageModule(renewDetails, userIp, ref recPeriods, ref isMPPRecurringInfinitely, subscription, ignoreUnifiedBillingCycle, false))
            {
                log.Error("Error while trying to get Price plan to renew");
                return response;
            }

            var currencyObj = Pricing.Module.GetCurrencyValues(groupId, renewDetails.Currency);
            ApiObjects.Country country = null;
            if (!string.IsNullOrEmpty(renewDetails.CountryName))
            {
                country = Utils.GetCountryByCountryName(groupId, renewDetails.CountryName);
            }

            response.EntitlementRenewal = new EntitlementRenewal()
            {
                Date = renewDetails.EndDate.Value,
                SubscriptionId = renewDetails.ProductId,
                PurchaseId = purchaseId,
                Price = new Price()
                {
                    m_dPrice = renewDetails.Price,
                    m_oCurrency = currencyObj,
                    countryId = country != null ? country.Id : 0,
                },
                GroupId = groupId,
                Id = purchaseId,
                UserId = userId
            };

            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public static UnifiedPaymentRenewalResponse GetUnifiedPaymentNextRenewal(BaseConditionalAccess cas, int groupId, long householdId, long unifiedPaymentId, long userId)
        {
            UnifiedPaymentRenewalResponse response = new UnifiedPaymentRenewalResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            string customData = string.Empty;

            int paymentgatewayId = 0;
            ProcessUnifiedState processState = ProcessUnifiedState.Renew;
            DateTime? processEndDate = null;
            bool result = GetProcessDetails(unifiedPaymentId, ref paymentgatewayId, ref processState, ref processEndDate);

            // get subscription purchase 
            DataTable subscriptionPurchaseDt = DAL.ConditionalAccessDAL.Get_SubscriptionPurchaseUnifiedForRenewal(groupId, householdId, unifiedPaymentId);

            // validate subscription received
            if (subscriptionPurchaseDt == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("GetUnifiedPaymentRenewal: failed to get unifiedBillingCycle from CB for householdId ={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }

            var renewDetailsListResponse = GetRenewDetailsList(subscriptionPurchaseDt, groupId, cas, out Dictionary<long, Subscription> subscriptionsMap, "GetUnifiedPaymentNextRenewal");
            if (!renewDetailsListResponse.HasObjects())// all resDetails were addon with none relevant base
            {
                log.DebugFormat("GetUnifiedPaymentRenewal: No subscriptions found for householdId={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }
            var renewDetailsList = renewDetailsListResponse.Objects;

            // get unified billing cycle for this household 
            var subscriptionCycle = Utils.GetSubscriptionCycle(groupId, (int)householdId, renewDetailsList[0].MaxVLCOfSelectedUsageModule);
            if (!subscriptionCycle.HasCycle || subscriptionCycle.UnifiedBillingCycle == null)
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no groupBillingCycle define in CB for groupId={0}", groupId);
                return response;
            }

            #region check addon subscriptions

            var removeSubscriptionCodes = new List<long>();
            var subscriptionsMapByType = subscriptionsMap.Values.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToList());

            if (subscriptionsMapByType.ContainsKey(SubscriptionType.AddOn))
            {
                foreach (Subscription subscription in subscriptionsMapByType[SubscriptionType.AddOn])
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

                            if (subscriptionsMapByType.ContainsKey(SubscriptionType.Base))
                            {
                                // check if one of the subscription are base in this unified cycle 
                                foreach (Subscription baseSubscription in subscriptionsMapByType[SubscriptionType.Base])
                                {
                                    List<long> baseSetIds = baseSubscription.GetSubscriptionSetIdsToPriority().Select(x => x.Key).ToList();
                                    if (baseSetIds.Where(x => addOnSetIds.Contains(x)).Count() > 0)
                                    {
                                        canPurchaseAddOn = true;
                                    }
                                }
                            }

                            if (!canPurchaseAddOn)
                            {
                                // change is recurring to false and call event handle- this renew subscription failed!                        

                                long nextEndDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(processEndDate.Value);
                                var subscriptionCode = int.Parse(subscription.m_SubscriptionCode);
                                RenewDetails rsDetail = renewDetailsList.FirstOrDefault(x => x.ProductId == subscriptionCode);
                                if (HandleRenewUnifiedSubscriptionFailed(cas, groupId, paymentgatewayId, householdId, subscription, rsDetail, 0, "AddOn with no BaseSubscription valid",
                                    string.Empty, nextEndDate))
                                {
                                    // save all SubscriptionCode to remove from subscription list 
                                    removeSubscriptionCodes.Add(subscriptionCode);
                                    // remove this renewDetails (its an AddOn)
                                    renewDetailsList.Remove(rsDetail);
                                }

                                log.ErrorFormat("failed renew subscription subscriptionCode: {0}, CanPurchaseAddOn return status code = {1}, status message = {2}", subscription.m_SubscriptionCode, status.Code, status.Message);
                            }
                        }
                    }
                }
            }

            // all resDetails were addon with none relevant base
            if (renewDetailsList.Count == 0)
            {
                log.DebugFormat("RenewUnifiedTransaction fail due to no addon subscriptions for householdid = {0} and paymentgatewayid = { 1 }", householdId, paymentgatewayId);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            // remove all not relevant subscriptions
            if (removeSubscriptionCodes.Count > 0)
            {
                foreach (var subCodeToRemove in removeSubscriptionCodes)
                {
                    subscriptionsMap.Remove(subCodeToRemove);
                }
            }

            #endregion
            
            Utils.GetMultiSubscriptionUsageModule(renewDetailsList, string.Empty, subscriptionsMap, cas, ref subscriptionCycle, (int)householdId, groupId, false);

            if (renewDetailsList.Count == 0)
            {
                log.ErrorFormat("GetUnifiedPaymentRenewal: Failed to get renewal data for householdId={0}, unifiedPaymentId={1}", householdId, unifiedPaymentId);
                return response;
            }

            // build response 
            var currencyObj = Core.Pricing.Module.GetCurrencyValues(groupId, renewDetailsList[0].Currency);
            ApiObjects.Country country = null;
            if (!string.IsNullOrEmpty(renewDetailsList[0].CountryName))
            {
                country = Utils.GetCountryByCountryName(groupId, renewDetailsList[0].CountryName);
            }

            response.UnifiedPaymentRenewal = new UnifiedPaymentRenewal()
            {
                Date = DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate),
                UnifiedPaymentId = unifiedPaymentId,
                Entitlements = new List<EntitlementRenewalBase>(),
                Price = new Price()
                {
                    m_oCurrency = currencyObj,
                    countryId = country != null ? country.Id : 0,
                },
                GroupId = groupId,
                Id = unifiedPaymentId,
                UserId = userId
            };

            EntitlementRenewalBase entitlementRenewal = null;
            foreach (var renewDetails in renewDetailsList)
            {
                entitlementRenewal = new EntitlementRenewalBase()
                {
                    PriceAmount = renewDetails.Price,
                    PurchaseId = renewDetails.PurchaseId,
                    SubscriptionId = renewDetails.ProductId,
                    GroupId = groupId,
                    Id = renewDetails.PurchaseId
                };
                response.UnifiedPaymentRenewal.Entitlements.Add(entitlementRenewal);
                response.UnifiedPaymentRenewal.Price.m_dPrice += renewDetails.Price;
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

            long userId = long.Parse(siteGuid);

            var renewalResponse = GetEntitlementNextRenewal(baseConditionalAccess, groupId, householdId, purchaseId, userId);

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
            if (Math.Abs(DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate) - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling RenewalReminder task." +
                    "Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate), logString);
                return true;
            }

            renewalResponse.EntitlementRenewal.Price.m_oCurrency.m_sCurrencySign = string.Empty;

            success = renewalResponse.EntitlementRenewal.Notify();

            return success;
        }

        internal static bool UnifiedRenewalReminder(BaseConditionalAccess baseConditionalAccess, int groupId, long householdId, long processId, long nextEndDate)
        {
            bool success = false;

            // log request
            string logString = string.Format("RenewalReminder request: processId {0}, endDateLong {1}", processId, nextEndDate);

            log.DebugFormat("Starting UnifiedRenewalReminder process. data: {0}", logString);

            // validate purchaseId
            if (processId <= 0)
            {
                // Illegal purchase ID  
                log.ErrorFormat("Illegal processId. data: {0}", logString);
                return true;
            }

            if (householdId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                return true;
            }

            Domain domain;
            ApiObjects.Response.Status domainStatus = Utils.ValidateDomain(groupId, (int)householdId, out domain);
            if (domain == null || domainStatus == null || domainStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("UnifiedRenewalReminder ValidateDomain householdId = {0} status.Code = {1} ", householdId, domainStatus.Code);
                return true;
            }

            long userId = domain.m_masterGUIDs[0];

            var unifiedPaymentResponse = GetUnifiedPaymentNextRenewal(baseConditionalAccess, groupId, householdId, processId, userId);

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

        private static bool UpdateProcessDetailsForRenewal(long processId, ref int paymentgatewayId, ref ProcessUnifiedState processPurchasesState,
                                                           ref DateTime? processEndDate, out DateTime? purchaseDate)
        {
            purchaseDate = null;
            DataRow dr = ConditionalAccessDAL.UpdateProcessDetailsForRenewal(processId);
            if (dr != null)
            {
                paymentgatewayId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PAYMENT_GATEWAY_ID");
                int state = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATE");
                processPurchasesState = (ProcessUnifiedState)state;
                processEndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                purchaseDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
            }

            return paymentgatewayId > 0 && processEndDate.HasValue;
        }

        private static GenericResponse<RenewDetails> GetRenewDetails(BaseConditionalAccess cas, string logString, ResponseStatus userValidStatus, long domainId, string userId, long purchaseId, int groupId, string billingGuid, ref bool shouldUpdateTaskStatus, out Subscription subscription, string userIp)
        {
            subscription = null;
            var response = new GenericResponse<RenewDetails>();
            

            var subscriptionPurchaseRow = ConditionalAccessDAL.Get_SubscriptionPurchaseForRenewal(groupId, purchaseId, billingGuid);
            if (subscriptionPurchaseRow == null)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("problem getting the subscription purchase. Data:{0}.", logString);
                shouldUpdateTaskStatus = false;
                return response;
            }

            var renewDetails = new RenewDetails()
            {
                ProductId = ODBCWrapper.Utils.ExtractInteger(subscriptionPurchaseRow, "SUBSCRIPTION_CODE"), // AKA subscription ID/CODE
                PurchaseId = purchaseId,
                BillingGuid = billingGuid,
                UserId = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "SITE_USER_GUID"),
                DomainId = domainId,
                ShouldSwitchToMasterUser = false,
                GroupId = groupId,
                Currency = "n/a",
                CustomData = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "CUSTOMDATA"),
                EndDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionPurchaseRow, "END_DATE"),
                StartDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionPurchaseRow, "START_DATE"),
                SubscriptionStatus = (SubscriptionPurchaseStatus)ODBCWrapper.Utils.ExtractInteger(subscriptionPurchaseRow, "subscription_status"),
            };

            // validate user ID
            if (renewDetails.UserId != userId)
            {
                // siteguid not equal to purchase siteguid
                log.ErrorFormat("siteguid {0} not equal to purchase siteguid {1}. data: {2}", userId, renewDetails.UserId, logString);
                response.SetStatus(eResponseStatus.OK);
                return response;
            }

            // check if we need to set shouldSwitchToMasterUser = true so we will update subscription details to master user instead of user where needed
            if (userValidStatus == ResponseStatus.UserSuspended)
            {
                userValidStatus = ResponseStatus.OK;
            }
            else if (userValidStatus == ResponseStatus.UserDoesNotExist)
            {
                renewDetails.ShouldSwitchToMasterUser = true;
                renewDetails.DomainId = ODBCWrapper.Utils.GetLongSafeVal(subscriptionPurchaseRow, "DOMAIN_ID");
                string masterSiteGuid = string.Empty;
                if (renewDetails.DomainId > 0)
                {
                    Domain domain = Utils.GetDomainInfo((int)renewDetails.DomainId, groupId);
                    if (domain != null && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
                    {
                        masterSiteGuid = domain.m_masterGUIDs.First().ToString();
                    }
                }

                if (string.IsNullOrEmpty(masterSiteGuid))
                {
                    // could not find a master user to replace the deleted user                   
                    log.ErrorFormat("User validation failed: UserDoesNotExist and no MasterUser to replace in renew, data: {0}", logString);
                    response.SetStatus(eResponseStatus.OK);
                    return response;
                }

                log.WarnFormat("SiteGuid: {0} does not exist, changing renew SiteGuid value to MasterSiteGuid: {1}", userId, masterSiteGuid);
                renewDetails.UserId = masterSiteGuid;
            }

            // check if response OK only if we know response is not UserDoesNotExist, shouldSwitchToMasterUser is set to false by default
            if (!renewDetails.ShouldSwitchToMasterUser && userValidStatus != ResponseStatus.OK)
            {
                // user validation failed
                ApiObjects.Response.Status status = Utils.SetResponseStatus(userValidStatus);
                log.ErrorFormat("User validation failed: {0}, data: {1}", status.Message, logString);
                response.SetStatus(eResponseStatus.OK);
                return response;
            }

            // validate household
            if (renewDetails.DomainId <= 0)
            {
                // illegal household ID
                log.ErrorFormat("Error: Illegal household, data: {0}", logString);
                response.SetStatus(eResponseStatus.OK);
                return response;
            }

            if (!TryGetSubscription(groupId, renewDetails.ProductId, out subscription, logString))
            {
                return response;
            }

            try
            {
                if (userValidStatus == ResponseStatus.OK && !string.IsNullOrEmpty(renewDetails.CustomData))
                {
                    if (!SetCustomDataForRenewDetails(groupId, renewDetails, out bool isDummy, out XmlNode theRequest))
                    {
                        return response;
                    }

                    if (isDummy)
                    {
                        if (HandleDummySubsciptionRenewal(cas, groupId, renewDetails, billingGuid, logString, userIp, theRequest))
                        {
                            response.SetStatus(eResponseStatus.OK);
                        }

                        return response;
                    }
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Renew: Error while getting data from xml, data: {0}, error: {1}", logString, exc.ToString());
                return response;
            }

            var renewDetailsRow = ConditionalAccessDAL.Get_RenewDetails(groupId, purchaseId, billingGuid);
            if (renewDetailsRow == null)
            {
                // transaction details weren't found
                log.ErrorFormat("Transaction details weren't found. Data:{0}.", logString);
                return response;
            }

            renewDetails.PaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "PAYMENT_NUMBER");
            renewDetails.NumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "number_of_payments");

            renewDetails.RecurringData = ConditionalAccessDAL.GetRecurringRenewDetails(purchaseId);
            if (renewDetails.RecurringData == null)
            {
                var couponCode = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "COUPON_CODE");
                renewDetails.RecurringData = EntitlementManager.InitializeRecurringRenewDetails(groupId, renewDetailsRow, purchaseId, subscription, couponCode);
            }
            renewDetails.PaymentNumber = Utils.CalcPaymentNumber(renewDetails.NumOfPayments, renewDetails.PaymentNumber, renewDetails.RecurringData.IsPurchasedWithPreviewModule);

            response.Object = renewDetails;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private static GenericListResponse<RenewDetails> GetRenewDetailsList(DataTable subscriptionPurchaseDt, int groupId, BaseConditionalAccess cas, out Dictionary<long, Subscription> subscriptionsMap, string logString)
        {
            var response = new GenericListResponse<RenewDetails>();
            subscriptionsMap = new Dictionary<long, Subscription>();

            try
            {
                DateTime minStartDate = DateTime.UtcNow;

                foreach (DataRow dr in subscriptionPurchaseDt.Rows)
                {
                    var renewDetails = new RenewDetails
                    {
                        ProductId = ODBCWrapper.Utils.ExtractInteger(dr, "subscription_code"),
                        PurchaseId = ODBCWrapper.Utils.ExtractInteger(dr, "id"),
                        BillingGuid = ODBCWrapper.Utils.ExtractString(dr, "billing_guid"),
                        //BillingTransactionId
                        ExternalTransactionId = ODBCWrapper.Utils.ExtractString(dr, "external_transaction_id"),
                        UserId = ODBCWrapper.Utils.ExtractString(dr, "site_user_guid"),
                        DomainId = ODBCWrapper.Utils.ExtractInteger(dr, "domain_id"),
                        ShouldSwitchToMasterUser = false,
                        GroupId = groupId,
                        Currency = ODBCWrapper.Utils.ExtractString(dr, "currency_cd"),
                        // price
                        PaymentMethodId = ODBCWrapper.Utils.ExtractInteger(dr, "payment_method_id"),
                        PaymentNumber = ODBCWrapper.Utils.ExtractInteger(dr, "payment_number"),
                        NumOfPayments = ODBCWrapper.Utils.ExtractInteger(dr, "number_of_payments"),
                        CustomData = ODBCWrapper.Utils.ExtractString(dr, "customdata"),
                        EndDate = ODBCWrapper.Utils.ExtractDateTime(dr, "end_date"),
                        CountryName = ODBCWrapper.Utils.ExtractString(dr, "country_code"), // country code on db is really country name 
                        SubscriptionStatus = (SubscriptionPurchaseStatus)ODBCWrapper.Utils.ExtractInteger(dr, "subscription_status"),
                        //----------------
                        //StartDate 
                        //MaxVLCOfSelectedUsageModule
                        //GracePeriodMinutes
                    };

                    renewDetails.CountryCode = Utils.GetCountryCodeByCountryName(groupId, renewDetails.CountryName);

                    if (!TryGetSubscription(groupId, renewDetails.ProductId, out Subscription subscription, logString))
                    {
                        return response;
                    }

                    if (subscription.m_MultiSubscriptionUsageModule != null && subscription.m_MultiSubscriptionUsageModule.Length > 0)
                    {
                        renewDetails.MaxVLCOfSelectedUsageModule = subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle;
                    }

                    subscriptionsMap.Add(renewDetails.ProductId, subscription);

                    renewDetails.RecurringData = ConditionalAccessDAL.GetRecurringRenewDetails(renewDetails.PurchaseId);
                    if (renewDetails.RecurringData == null)
                    {
                        var couponCode = ODBCWrapper.Utils.ExtractString(dr, "coupon_code");
                        renewDetails.RecurringData = EntitlementManager.InitializeRecurringRenewDetails(groupId, dr, renewDetails.PurchaseId, subscription, renewDetails.BillingGuid, couponCode);
                    }

                    renewDetails.PaymentNumber = Utils.CalcPaymentNumber(renewDetails.NumOfPayments, renewDetails.PaymentNumber, renewDetails.RecurringData.IsPurchasedWithPreviewModule);
                    if (renewDetails.NumOfPayments > 0 && renewDetails.PaymentNumber > renewDetails.NumOfPayments)
                    {
                        // Subscription ended
                        var logSubscriptionEnded = string.Format("Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}, subscriptionID={3}, billingGuid={4}",
                                                                 renewDetails.NumOfPayments, renewDetails.PaymentNumber, renewDetails.NumOfPayments, renewDetails.ProductId, renewDetails.BillingGuid);
                        log.Error(logSubscriptionEnded);
                        cas.WriteToUserLog(renewDetails.UserId, logSubscriptionEnded);

                        continue; // won't insert this row details to the list ! 
                    }
                    renewDetails.PaymentNumber++;

                    response.Objects.Add(renewDetails);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("BuildSubscriptionPurchaseDetails failed ex = {0}", ex);
            }

            return response;
        }

        private static bool TryGetSubscription(int groupId, int productId, out Subscription subscription, string logString)
        {
            subscription = null;

            try
            {
                subscription = Pricing.Module.Instance.GetSubscriptionData(groupId, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while trying to fetch subscription data. data: {0}", logString), ex);
                return false;
            }

            // validate subscription
            if (subscription == null)
            {
                log.Error(string.Format("subscription wasn't found. productId {0}, data: {1}", productId, logString));
                return false;
            }

            log.DebugFormat("Subscription data received. data: {0}", logString);
            return true;
        }

        private static bool SetCustomDataForRenewDetails(int groupId, RenewDetails renewDetails, out bool isDummy, out XmlNode theRequest)
        {
            isDummy = false;
            theRequest = null;

            try
            {
                if (!string.IsNullOrEmpty(renewDetails.CustomData))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(renewDetails.CustomData);
                    theRequest = doc.FirstChild;
                    // previousPurchaseCurrencyCode, previousPurchaseCountryCode and previousPurchaseCountryName will be used later for getting the correct priceCodeData 
                    renewDetails.PreviousPurchaseCurrencyCode = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                    renewDetails.CountryName = XmlUtils.GetSafeValue(BaseConditionalAccess.COUNTRY_CODE, ref theRequest);
                    renewDetails.CountryCode = Utils.GetCountryCodeByCountryName(groupId, renewDetails.CountryName);
                    isDummy = XmlUtils.IsNodeExists(ref theRequest, BaseConditionalAccess.DUMMY);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("SetCustomDataForRenewDetails: Error while getting data from custom data xml, PurchaseId: {0}, householdId: {1}", renewDetails.PurchaseId, renewDetails.DomainId), ex);
                return false;
            }

            return true;
        }
    }

    public class UnifiedProcess
    {
        public long Id { get; set; }
        public DateTime EndDate { get; set; }
        public bool isNew { get; set; }
    }
}
