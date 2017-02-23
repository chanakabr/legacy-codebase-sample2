using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using Billing;
using CachingProvider.LayeredCache;
using ConditionalAccess.Modules;
using DAL;
using KLogMonitor;
using Pricing;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TVinciShared;
using Users;
using WS_API;
using WS_Billing;
using WS_Pricing;

namespace ConditionalAccess
{
    public class RenewManager
    {
        #region Consts

        private const string ILLEGAL_CONTENT_ID = "Illegal content ID";
        private const string MAX_USAGE_MODULE = "mumlc";
        protected const string ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION = "PROCESS_RENEW_SUBSCRIPTION\\{0}";        

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
            userValidStatus = Utils.ValidateUser(groupId, siteguid, ref householdId);

            // get end date
            DateTime endDate = ODBCWrapper.Utils.ExtractDateTime(subscriptionPurchaseRow, "END_DATE");

            // validate renewal did not already happened
            if (Math.Abs(TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate) - nextEndDate) > 60)
            {
                // subscription purchase wasn't found
                log.ErrorFormat("Subscription purchase last end date is not the same as next the new end date - canceling renew task. Purchase ID: {0}, sub end_date: {1}, data: {2}",
                    purchaseId, TVinciShared.DateUtils.DateTimeToUnixTimestamp(endDate), logString);
                return true;
            }

            string previousPurchaseCurrencyCode = string.Empty;
            string previousPurchaseCountryCode = string.Empty;
            #region Dummy

            try
            {
                customData = ODBCWrapper.Utils.ExtractString(subscriptionPurchaseRow, "CUSTOMDATA"); // AKA subscription ID/CODE

                if (userValidStatus == ResponseStatus.OK && !string.IsNullOrEmpty(customData))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(customData);
                    XmlNode theRequest = doc.FirstChild;
                    // previousPurchaseCurrencyCode and previousPurchaseCountryCode will be used later for getting the correct priceCodeData 
                    previousPurchaseCurrencyCode = XmlUtils.GetSafeValue(BaseConditionalAccess.CURRENCY, ref theRequest);
                    previousPurchaseCountryCode = XmlUtils.GetSafeValue(BaseConditionalAccess.COUNTRY_CODE, ref theRequest);
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

            string wsUsername = string.Empty;
            string wsPassword = string.Empty;
            Subscription subscription = null;
            try
            {
                using (mdoule m = new mdoule())
                {
                    Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref wsUsername, ref wsPassword);
                    subscription = m.GetSubscriptionData(wsUsername, wsPassword, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
                }
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
            Compensation compensation = ConditionalAccessDAL.GetSubscriptionCompensation(purchaseId);

            // get MPP
            int recPeriods = 0;
            bool isMPPRecurringInfinitely = false;
            int maxVLCOfSelectedUsageModule = 0;
            double price = 0;
            string currency = "n/a";

            try
            {
                cas.GetMultiSubscriptionUsageModule(siteguid, userIp, (int)purchaseId, paymentNumber, totalNumOfPayments, numOfPayments, isPurchasedWithPreviewModule,
                        ref price, ref customData, ref currency, ref recPeriods, ref isMPPRecurringInfinitely, ref maxVLCOfSelectedUsageModule,
                        ref couponCode, subscription, compensation, previousPurchaseCountryCode, previousPurchaseCountryCode);
            }
            catch (Exception ex)
            {
                // "Error while trying to get MPP
                log.Error("Error while trying to get MPP", ex);
                return false;
            }

            // call billing process renewal
            string billingUserName = string.Empty;
            string billingPassword = string.Empty;
            module wsBillingService = null;
            Utils.InitializeBillingModule(ref wsBillingService, groupId, ref billingUserName, ref billingPassword);
            TransactResult transactionResponse = null;
            try
            {
                transactionResponse = wsBillingService.ProcessRenewal(billingUserName, billingPassword, siteguid, householdId, price, currency,
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
                        siteguid, purchaseId, logString, productId, subscription, householdId, 0, transactionResponse.Status.Message);
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
                            totalNumOfPayments, subscription, customData, maxVLCOfSelectedUsageModule, billingUserName, billingPassword, wsBillingService, transactionResponse);
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
                                if (!ConditionalAccessDAL.UpdateSubscriptionCompernsationUse(compensation.Id, transactionResponse.TransactionID, compensation.Renewals + 1))
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
                            siteguid, purchaseId, billingGuid, logString, productId, endDate, householdId, shouldSwitchToMasterUser, price, currency,
                            billingUserName, billingPassword, wsBillingService);
                    }
                    break;
                case eTransactionState.Failed:
                    {
                        // renew subscription failed!
                        res = HandleRenewSubscriptionFailed(cas, groupId,
                            siteguid, purchaseId, logString, productId, subscription, householdId, transactionResponse.FailReasonCode);
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

        //protected internal static bool HandleDummySubsciptionRenewal(BaseConditionalAccess cas, int groupId, string siteguid, long purchaseId, string billingGuid,
        //    string logString, long householdId, string userIp, long productId, XmlNode theRequest)
        //{
        //    bool saveHistory = XmlUtils.IsNodeExists(ref theRequest, HISTORY);
        //    string deviceName = XmlUtils.GetSafeValue(DEVICE_NAME, ref theRequest);
        //    int recurringNumber = 0;
        //    int numOfPayments = 0;
        //    if (!int.TryParse(XmlUtils.GetSafeValue(RECURRING_NUMBER, ref theRequest), out recurringNumber))
        //    {
        //        // Subscription ended
        //        log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse recurringNumber,  data: {0}", logString);
        //        cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
        //            productId, numOfPayments, recurringNumber, numOfPayments, billingGuid));
        //        return false;
        //    }

        //    if (!int.TryParse(XmlUtils.GetSafeParValue("//p", "o", ref theRequest), out numOfPayments))
        //    {
        //        // Subscription ended
        //        log.ErrorFormat("Renew Dummy GrantSubscription failed, error at parse //p o,  data: {0}", logString);
        //        cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
        //            productId, numOfPayments, recurringNumber, numOfPayments, billingGuid));
        //        return false;

        //    }
        //    recurringNumber = Utils.CalcPaymentNumber(numOfPayments, recurringNumber, false);
        //    if (numOfPayments > 0 && recurringNumber > numOfPayments)
        //    {
        //        // Subscription ended
        //        log.ErrorFormat("Subscription ended. numOfPayments={0}, paymentNumber={1}, numOfPayments={2}", numOfPayments, recurringNumber, numOfPayments);
        //        cas.WriteToUserLog(userId, string.Format("Subscription ended. subscriptionID = {0}, numOfPayments={1}, paymentNumber={2}, numOfPayments={3}, billingGuid={4}",
        //            productId, numOfPayments, recurringNumber, numOfPayments, billingGuid));
        //        return true;
        //    }

        //    // calculate payment (recurring) number
        //    recurringNumber++;

        //    /// call GrantSubsription
        //    var res = GrantManager.GrantSubscription(cas, groupId, userId, householdId, (int)productId, userIp, deviceName, saveHistory, recurringNumber, null, null, GrantContext.Renew);
        //    if (res.Code == (int)eResponseStatus.OK)
        //    {
        //        log.DebugFormat("Renew Dummy GrantSubscription Succeeded, data: {0}", logString);

        //        // Try to cancel subscription
        //        if (ConditionalAccessDAL.CancelSubscription((int)purchaseId, groupId, userId, productId.ToString()) == 0)
        //        {
        //            log.Error("Error while trying to update subscription");
        //            return false;
        //        }
        //        else
        //        {
        //            log.Debug("Subscription was updated");
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        log.DebugFormat("Renew Dummy GrantSubscription failed, data: {0}", logString);
        //        return true;
        //    }
        //}

        protected internal static bool HandleRenewSubscriptionFailed(BaseConditionalAccess cas, int groupId,
            string siteguid, long purchaseId, string logString, long productId,
            Subscription subscription, long domainId, int failReasonCode, string billingSettingError = null)
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

            return true;
        }

        protected internal static bool HandleRenewSubscriptionPending(BaseConditionalAccess cas, int groupId,
            string siteguid, long purchaseId, string billingGuid, string logString, long productId, DateTime endDate,
            long householdId, bool shouldSwitchToMasterUser, double price, string currency, string billingUserName, string billingPassword, module wsBillingService)
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

                paymentGatewayResponse = wsBillingService.GetPaymentGatewayByBillingGuid(billingUserName, billingPassword, householdId, billingGuid);

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
                    log.DebugFormat("New task created (upon renew pending response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);

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
            string billingUserName, string billingPassword, module wsBillingService, TransactResult transactionResponse)
        {
            // renew subscription success!
            log.DebugFormat("Transaction renew success. data: {0}", logString);

            // get billing gateway
            PaymentGateway paymentGateway = null;
            try
            {
                paymentGateway = wsBillingService.GetPaymentGatewayByBillingGuid(billingUserName, billingPassword, householdId, billingGuid);
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
                // end wasn't retuned - get next end date from MPP
                endDate = Utils.GetEndDateTime(endDate, maxVLCOfSelectedUsageModule);
                log.DebugFormat("New end-date was updated according to MPP. EndDate={0}", endDate);
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
                ApiDAL.Update_PurchaseIDInBillingTransactions(transactionResponse.TransactionID, purchaseId);
            else
                log.Error("Error while trying update billing_transactions subscriptions_purchased reference");

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
                log.DebugFormat("New task created (upon renew success response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);

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
            string billingUserName = string.Empty;
            string billingPassword = string.Empty;
            module wsBillingService = null;
            Utils.InitializeBillingModule(ref wsBillingService, groupId, ref billingUserName, ref billingPassword);
            try
            {
                List<PaymentDetails> paymentDetails = wsBillingService.GetPaymentDetails(billingUserName, billingPassword, new List<string>() { billingGuid });
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
                string pricingUserName = string.Empty;
                string pricingPassword = string.Empty;
                Subscription subscription = null;
                try
                {
                    using (mdoule m = new mdoule())
                    {
                        Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref pricingUserName, ref pricingPassword);
                        subscription = m.GetSubscriptionData(pricingUserName, pricingPassword, productId.ToString(), string.Empty, string.Empty, string.Empty, false);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error while trying to fetch subscription data. data: {0}", logString), ex);
                    return false;
                }

                string itemName = subscription.m_sObjectVirtualName;
                API apiWS = null;
                try
                {
                    apiWS = new API();

                    GiftCardReminderMailRequest giftCardRequest =
                        GetGiftCardReminderTemplate(groupId, user, itemName, endDate);

                    if (giftCardRequest != null)
                    {
                        log.DebugFormat("params for gift card reminder mail ws_cas .m_sSubject={0}, houseHoldUser.m_sSiteGUID={1}, purchaseRequest.m_sTemplateName={2}",
                            giftCardRequest.m_sSubject, userId, giftCardRequest.m_sTemplateName);

                        if (!string.IsNullOrEmpty(giftCardRequest.m_sTemplateName))
                        {
                            string sWSUserName = string.Empty;
                            string sWSPass = string.Empty;
                            Utils.GetWSCredentials(groupId, eWSModules.API, ref sWSUserName, ref sWSPass);
                            success = apiWS.SendMailTemplate(sWSUserName, sWSPass, giftCardRequest);

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
                finally
                {
                    if (apiWS != null)
                    {
                        apiWS.Dispose();
                    }
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
                log.DebugFormat("New task created (upon renew success response). Next renewal date: {0}, data: {1}", nextRenewalDate, data);

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
            Subscription[] subscriptions = Utils.GetSubscriptionsDataWithCaching(new List<long>(1) { productId }, pricingUsername, pricingPassword, groupId);
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
                module wsBillingService = null;
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                BillingResponse billingResponse = new BillingResponse();
                billingResponse.m_oStatus = BillingResponseStatus.UnKnown;
                
                cas.InitializeBillingModule(ref wsBillingService, ref sWSUserName, ref sWSPass);
                billingResponse = cas.HandleCCChargeUser(sWSUserName, sWSPass, userId, double.Parse(price), currency, userIp, customData,
                       newRecurringNumber, numOfPayments, string.Empty, string.Empty, string.Empty, true, false, ref wsBillingService);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success || !long.TryParse(billingResponse.m_sRecieptCode, out billingTransactionID))
                {
                    return false;
                }
            
            }

            return HandleRenewGrantedSubscription(cas, groupId, userId, purchaseId, billingGuid, productId, ref endDate, householdId,
               double.Parse(price), currency, newRecurringNumber, numOfPayments, subscription, theRequest.InnerXml, int.Parse(mumlc), billingTransactionID);
            
        }
    }
}
