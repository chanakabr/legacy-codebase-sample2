using APILogic.ConditionalAccess.Managers;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.SubscriptionSet;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.ConditionalAccess
{
    public class PurchaseManager
    {
        #region Consts

        private const string ILLEGAL_CONTENT_ID = "Illegal content ID";
        protected const string ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION = "PROCESS_RENEW_SUBSCRIPTION\\{0}";
        

        #endregion

        #region DomainSubscriptionPurchase

        /// <summary>
        /// Partially defines domains purchase of a subscription bundle
        /// </summary>
        [Serializable]
        public class DomainSubscriptionPurchaseDetails : Core.ConditionalAccess.Utils.UserBundlePurchase
        {
            public long PurchaseId { get; set; }
            public double Price { get; set; }
            public string CurrencyCode { get; set; }
            public bool IsRecurring { get; set; }
            public string BillingGuid { get; set; }
            public bool IsFirstSubscriptionSetModify { get; set; }

            public DomainSubscriptionPurchaseDetails()
                : base()
            {
                this.PurchaseId = 0;
                this.Price = 0;
                this.CurrencyCode = string.Empty;
                IsRecurring = false;
                this.BillingGuid = string.Empty;
                this.IsFirstSubscriptionSetModify = false;
            }

            public DomainSubscriptionPurchaseDetails(Core.ConditionalAccess.Utils.UserBundlePurchase bundlePurchae, long purchaseId, double price, string currencyCode, bool isRecurring,
                                                     string billingGuid, bool isFirstSubscriptionSetModify)
                : base()
            {
                this.sBundleCode = bundlePurchae.sBundleCode;
                this.nWaiver = bundlePurchae.nWaiver;
                this.dtPurchaseDate = bundlePurchae.dtPurchaseDate;
                this.dtEndDate = bundlePurchae.dtEndDate;
                this.nNumOfUses = bundlePurchae.nNumOfUses;
                this.nMaxNumOfUses = bundlePurchae.nMaxNumOfUses;
                this.PurchaseId = purchaseId;
                this.Price = price;
                this.CurrencyCode = currencyCode;
                this.IsRecurring = isRecurring;
                this.BillingGuid = billingGuid;
                this.IsFirstSubscriptionSetModify = isFirstSubscriptionSetModify;
            }
        }

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Static Methods

        internal static Status SubscriptionSetModifySubscription(BaseConditionalAccess cas, int groupId, string userId, long domainId, double price, string currencyCode, int productId, string couponCode,
                                                                string userIp, string udid, int paymentGatewayId, int paymentMethodId, string adapterData, bool isUpgrade, ref TransactionResponse transactionResponse)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            transactionResponse = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            // log request
            string logString = string.Format(@"SubscriptionSetModifySubscription request: groupId {0}, userId {1}, domainId {2}, price {3}, currencyCode {4}, productId {5}, couponCode {6}, userIp {7}, udid {8},
                                            paymentGatewayId {9}, paymentMethodId {10}, adapterData {11}, isUpgrade {12}", groupId, !string.IsNullOrEmpty(userId) ? userId : string.Empty, domainId, price,
                                            !string.IsNullOrEmpty(currencyCode) ? currencyCode : string.Empty, productId, !string.IsNullOrEmpty(couponCode) ? couponCode : string.Empty,
                                            !string.IsNullOrEmpty(userIp) ? userIp : string.Empty, !string.IsNullOrEmpty(udid) ? udid : string.Empty, paymentGatewayId, paymentMethodId, adapterData, isUpgrade);
            log.DebugFormat(logString);

            // validate siteguid
            if (string.IsNullOrEmpty(userId))
            {
                response.Message = "Illegal user ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Message, logString);
                return response;
            }

            // validate currency
            if (string.IsNullOrEmpty(currencyCode))
            {
                response.Message = "Illegal currency";
                log.ErrorFormat("Error: {0}, data: {1}", response.Message, logString);
                return response;
            }

            // validate productId
            if (productId < 1)
            {
                response.Message = "Illegal product ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Message, logString);
                return response;
            }

            try
            {
                // validate user
                ResponseStatus userValidStatus = ResponseStatus.OK;
                Core.Users.User user;
                userValidStatus = Utils.ValidateUser(groupId, userId, ref domainId, out user);

                if (userValidStatus != ResponseStatus.OK || user == null || user.m_oBasicData == null)
                {
                    // user validation failed
                    response = Utils.SetResponseStatus(userValidStatus);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Message, logString);
                    return response;
                }

                // validate household
                if (domainId < 1)
                {
                    response.Message = "Illegal household";
                    log.ErrorFormat("Error: {0}, data: {1}", response.Message, logString);
                    return response;
                }

                CouponData couponData = null;
                // coupon validation
                if (!string.IsNullOrEmpty(couponCode))
                {
                    couponData = Utils.GetCouponData(groupId, couponCode);

                    if (couponData == null)
                    {
                        response.Message = "Coupon Not Valid";
                        response.Code = (int)eResponseStatus.CouponNotValid;
                        log.ErrorFormat("Error: {0}, data: {1}", response.Message, logString);
                        return response;
                    }
                }

                string country = string.Empty;
                if (!string.IsNullOrEmpty(userIp))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription subscription = Utils.GetSubscription(groupId, productId);

                if (subscription == null)
                {
                    response.Message = "ProductId doesn't exist";
                    return response;
                }

                if (subscription.m_UserTypes != null && subscription.m_UserTypes.Length > 0 && !subscription.m_UserTypes.Contains(user.m_oBasicData.m_UserType))
                {
                    response = new Status((int)eResponseStatus.SubscriptionNotAllowedForUserType, eResponseStatus.SubscriptionNotAllowedForUserType.ToString());
                    return response;
                }

                if (!subscription.m_bIsRecurring)
                {
                    response = new Status((int)eResponseStatus.SubscriptionNotRenewable, eResponseStatus.SubscriptionNotRenewable.ToString());
                    return response;
                }
                
                if (subscription.SubscriptionSetIdsToPriority == null || subscription.SubscriptionSetIdsToPriority.Count == 0)
                {
                    response = new Status((int)eResponseStatus.CanOnlyUpgradeOrDowngradeRecurringSubscriptionInTheSameSubscriptionSet,
                                                "Can only upgrade or downgrade subscription in the same subscriptionSet");
                    return response;
                }
                
                KeyValuePair<long, int> setAndPriority = subscription.GetSubscriptionSetIdsToPriority().First();
                Subscription subscriptionInTheSameSet = null;
                DomainSubscriptionPurchaseDetails previousSubsriptionPurchaseDetails = null;
                if (!IsEntitlementContainSameSetSubscription(groupId, domainId, productId, setAndPriority.Key, ref subscriptionInTheSameSet, ref previousSubsriptionPurchaseDetails))
                {
                    response = new Status((int)eResponseStatus.CanOnlyUpgradeOrDowngradeRecurringSubscriptionInTheSameSubscriptionSet,
                    "Can only upgrade or downgrade recurring subscription in the same subscriptionSet");
                    return response;
                }

                if (!previousSubsriptionPurchaseDetails.IsFirstSubscriptionSetModify)
                {
                    response = new Status((int)eResponseStatus.CanOnlyUpgradeOrDowngradeSubscriptionOnce, eResponseStatus.CanOnlyUpgradeOrDowngradeSubscriptionOnce.ToString());
                    return response;
                }

                if (!previousSubsriptionPurchaseDetails.IsRecurring)
                {
                    response = new Status((int)eResponseStatus.CanOnlyUpgradeOrDowngradeRecurringSubscriptionInTheSameSubscriptionSet,
                    "Can only upgrade or downgrade recurring subscription in the same subscriptionSet");
                    return response;
                }

                Dictionary<long, int> subscriptionInTheSameSetSubscriptionIds = subscriptionInTheSameSet.GetSubscriptionSetIdsToPriority();
                if (isUpgrade && setAndPriority.Value < subscriptionInTheSameSetSubscriptionIds[setAndPriority.Key])
                {
                    response = new Status((int)eResponseStatus.CanOnlyUpgradeSubscriptionWithHigherPriority, eResponseStatus.CanOnlyUpgradeSubscriptionWithHigherPriority.ToString());
                    return response;
                }
                else if (!isUpgrade && setAndPriority.Value > subscriptionInTheSameSetSubscriptionIds[setAndPriority.Key])
                {
                    response = new Status((int)eResponseStatus.CanOnlyDowngradeSubscriptionWithLowerPriority, eResponseStatus.CanOnlyDowngradeSubscriptionWithLowerPriority.ToString());
                    return response;
                }

                PaymentDetails paymentDetails = null;
                ApiObjects.Response.Status verificationStatus = Core.Billing.Module.GetPaymentGatewayVerificationStatus(groupId, previousSubsriptionPurchaseDetails.BillingGuid, ref paymentDetails);
                if (verificationStatus == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }
                else if (verificationStatus.Code != (int)eResponseStatus.OK || paymentDetails == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotValid, "couldn't get previous payment details");
                    log.ErrorFormat("Verification payment gateway does not support SubscriptionSetModifySubscription. billingGuid = {0}", previousSubsriptionPurchaseDetails.BillingGuid);
                    return response;
                }
                // Update payment details to be as the previous subscription purchase
                else if (paymentGatewayId == 0)                                
                {
                    paymentGatewayId = paymentDetails.PaymentGatewayId;
                    paymentMethodId = paymentDetails.PaymentMethodId;
                }

                bool isGiftCard = false;
                Price priceResponse = null;
                
                priceResponse = Utils.GetSubscriptionFinalPrice(groupId, productId.ToString(), userId, couponCode,
                    ref priceReason, ref subscription, country, string.Empty, udid, userIp, currencyCode, true);

                if (priceReason != PriceReason.ForPurchase)
                {
                    response.Message = "Product not for purchase";
                    return response;
                }

                // downgrade logic
                if (!isUpgrade)
                {
                    return Downgrade(cas, groupId, userId, domainId, priceResponse.m_dPrice, currencyCode, productId, couponCode, userIp, udid, paymentGatewayId, paymentMethodId,
                                     adapterData, subscriptionInTheSameSet, previousSubsriptionPurchaseDetails);
                }
                // upgrade logic
                else
                {
                    return Upgrade(cas, groupId, userId, domainId, currencyCode, productId, couponCode, userIp, udid, paymentGatewayId, paymentMethodId, adapterData, ref transactionResponse,
                                    logString, couponData, country, priceReason, subscription, ref isGiftCard, ref priceResponse, subscriptionInTheSameSet, previousSubsriptionPurchaseDetails);
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("SubscriptionSetModifySubscription Error. data: {0}", logString, ex));
                response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
        }

        private static Status Upgrade(BaseConditionalAccess cas, int groupId, string userId, long domainId, string currencyCode, int productId, string couponCode, string userIp, string udid,
                                                    int paymentGatewayId, int paymentMethodId, string adapterData, ref TransactionResponse transactionResponse, string logString, CouponData couponData,
                                                    string country, PriceReason priceReason, Subscription subscription, ref bool isGiftCard, ref Price priceResponse, Subscription subscriptionInTheSameSet,
                                                    DomainSubscriptionPurchaseDetails previousSubsriptionPurchaseDetails)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            transactionResponse.State = eTransactionState.Failed.ToString();
            if (string.IsNullOrEmpty(previousSubsriptionPurchaseDetails.CurrencyCode))
            {
                log.ErrorFormat("Failed to get previous subscription purchase currency code for groupId: {0}, purchaseId: {1}", groupId, previousSubsriptionPurchaseDetails.PurchaseId);
                response = new Status((int)eResponseStatus.Error, "Failed to get previous subscription currency code");
            }

            if (previousSubsriptionPurchaseDetails.CurrencyCode != currencyCode)
            {
                response = new Status((int)eResponseStatus.CanOnlyUpgradeSubscriptionWithTheSameCurrencyAsCurrentSubscription,
                                            eResponseStatus.CanOnlyUpgradeSubscriptionWithTheSameCurrencyAsCurrentSubscription.ToString());
                return response;
            }
                       
            if (!CalculateUpgradePrice(cas, domainId, subscriptionInTheSameSet, ref priceResponse, previousSubsriptionPurchaseDetails))
            {
                log.ErrorFormat("Failed upgrading subscription, domainId: {0}, oldSubscriptionId: {1}, newSubscriptionId: {2}", domainId, subscriptionInTheSameSet.m_ProductCode, productId);
                response = new Status((int)eResponseStatus.Error, "Failed calculating upgrade price");
            }

            // purchase new subscription

            if (couponData != null && couponData.m_CouponStatus == CouponsStatus.Valid && couponData.m_oCouponGroup != null && couponData.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard &&
                ((subscription.m_oCouponsGroup != null && subscription.m_oCouponsGroup.m_sGroupCode == couponData.m_oCouponGroup.m_sGroupCode) ||
                 (subscription.CouponsGroups != null && subscription.CouponsGroups.Count() > 0 && subscription.CouponsGroups.Where(x => x.m_sGroupCode == couponData.m_oCouponGroup.m_sGroupCode).Count() > 0)))
            {
                isGiftCard = true;
                priceResponse = new Price()
                {
                    m_dPrice = 0.0,
                    m_oCurrency = new Currency()
                    {
                        m_sCurrencyCD3 = currencyCode
                    }
                };
            }

            bool couponFullDiscount = (priceReason == PriceReason.Free && couponCode != null);

            if ((priceReason == PriceReason.ForPurchase || couponFullDiscount) || (isGiftCard && (priceReason == PriceReason.ForPurchase || priceReason == PriceReason.Free)))
            {
                // item is for purchase
                if (priceResponse != null &&
                    //priceResponse.m_dPrice == price &&
                    priceResponse.m_oCurrency.m_sCurrencyCD3 == currencyCode)
                {
                    // price is validated, create custom data
                    string customData = cas.GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, userId, priceResponse.m_dPrice, currencyCode,
                                                                        couponCode, userIp, country, string.Empty, udid, string.Empty, string.Empty, false);

                    // create new GUID for billing transaction
                    string billingGuid = Guid.NewGuid().ToString();

                    // purchase
                    if (couponFullDiscount || isGiftCard)
                    {
                        transactionResponse = HandleFullCouponPurchase(cas, groupId, userId, priceResponse.m_dPrice, currencyCode, userIp,
                            customData, productId, eTransactionType.Subscription, billingGuid, 0, isGiftCard);
                    }
                    else
                    {
                        transactionResponse = HandlePurchase(cas, groupId, userId, domainId, priceResponse.m_dPrice, currencyCode, userIp, customData, productId,
                                                  eTransactionType.Subscription, billingGuid, paymentGatewayId, 0, paymentMethodId, adapterData);
                    }

                    if (transactionResponse != null && transactionResponse.Status != null)
                    {
                        // Status OK + (State OK || State Pending) = grant entitlement
                        if (transactionResponse.Status.Code == (int)eResponseStatus.OK &&
                           (transactionResponse.State.Equals(eTransactionState.OK.ToString()) ||
                            transactionResponse.State.Equals(eTransactionState.Pending.ToString())))
                        {
                            // purchase passed
                            long purchaseID = 0;

                            // update entitlement date
                            DateTime entitlementDate = DateTime.UtcNow;
                            DateTime? endDate = null;
                            transactionResponse.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                            if (isGiftCard)
                            {
                                endDate = CalculateGiftCardEndDate(cas, couponData, subscription, entitlementDate);
                            }

                            // grant entitlement
                            bool handleBillingPassed =
                                cas.HandleSubscriptionBillingSuccess(ref transactionResponse, userId, domainId, subscription, priceResponse.m_dPrice, currencyCode, couponCode,
                                    userIp, country, udid, long.Parse(transactionResponse.TransactionID), customData, productId, billingGuid.ToString(),
                                    false, subscription.m_bIsRecurring, entitlementDate, ref purchaseID, ref endDate, SubscriptionPurchaseStatus.OK);

                            if (handleBillingPassed && endDate.HasValue)
                            {
                                cas.WriteToUserLog(userId, string.Format("Subscription Purchase, productId:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                    productId, purchaseID, transactionResponse.TransactionID));

                                // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                                if (subscription.m_nDomainLimitationModule != 0)
                                {
                                    cas.UpdateDLM(domainId, subscription.m_nDomainLimitationModule);
                                }

                                if (subscription.m_bIsRecurring)
                                {
                                    //DateTime nextRenewalDate = endDate.Value.AddMinutes(-5); // default  

                                    long endDateUnix = 0;

                                    if (endDate != null && endDate.HasValue)
                                    {
                                        endDateUnix = TVinciShared.DateUtils.DateTimeToUnixTimestamp((DateTime)endDate);
                                    }
                                    
                                    DateTime nextRenewalDate = endDate.Value;

                                    if (!isGiftCard)
                                    {
                                        // call billing process renewal
                                        try
                                        {
                                            nextRenewalDate = endDate.Value.AddMinutes(-5); // default  

                                            PaymentGateway paymentGatewayResponse = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, domainId, billingGuid);

                                            if (paymentGatewayResponse == null)
                                            {
                                                // error getting PG
                                                log.Error("Error getting the PG - GetPaymentGatewayByBillingGuid");
                                            }
                                            else
                                            {
                                                nextRenewalDate = endDate.Value.AddMinutes(paymentGatewayResponse.RenewalStartMinutes);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error("Error while trying to get the PG", ex);
                                        }
                                    }
                                    else
                                    {
                                        PurchaseManager.SendGiftCardReminderEmails(cas, groupId, endDateUnix, nextRenewalDate, userId, domainId, purchaseID, billingGuid);
                                    }

                                    // enqueue renew transaction
                                    #region Renew transaction message in queue

                                    RenewTransactionsQueue queue = new RenewTransactionsQueue();

                                    RenewTransactionData data = new RenewTransactionData(groupId, userId, purchaseID, billingGuid, endDateUnix, nextRenewalDate);
                                    bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                                    if (!enqueueSuccessful)
                                    {
                                        log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                                    }
                                    else
                                    {
                                        PurchaseManager.SendRenewalReminder(data, domainId);
                                        log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}",
                                            nextRenewalDate, data);
                                    }

                                    #endregion
                                }

                                // build notification message
                                var dicData = new Dictionary<string, object>()
                                    {
                                        {"SubscriptionCode", productId},
                                        {"BillingTransactionID", transactionResponse.TransactionID},
                                        {"SiteGUID", userId},
                                        {"PurchaseID", purchaseID},
                                        {"CouponCode", couponCode},
                                        {"CustomData", customData}
                                    };

                                // notify purchase
                                if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData))
                                {
                                    log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", transactionResponse.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase passed, entitlement failed
                                transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed but entitlement failed");
                                log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed - received error status
                            log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // purchase failed - no status error
                        transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                        log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
                    }
                }
                else
                {
                    // incorrect price
                    transactionResponse.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The price of the request is not the actual price");
                    log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
                }
            }
            else
            {
                // item not for purchase
                transactionResponse.Status = Utils.SetResponseStatus(priceReason);
                log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
            }


            if (transactionResponse != null && transactionResponse.Status != null)
            {
                response = new Status(transactionResponse.Status.Code, transactionResponse.Status.Message);
                if (transactionResponse.Status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetPurchaseInvalidationKey(domainId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on Purchase key = {0}", invalidationKey);
                    }

                    // cancel existing subscription
                    Status cancelSubscriptionStatus = cas.CancelServiceNow((int)domainId, int.Parse(subscriptionInTheSameSet.m_SubscriptionCode), eTransactionType.Subscription, true);
                    if (cancelSubscriptionStatus == null && cancelSubscriptionStatus.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed cas.CancelSubscriptionRenewal for domainId: {0}, subscriptionCode: {1}", domainId, subscriptionInTheSameSet.m_SubscriptionCode);
                        response = cancelSubscriptionStatus != null ? cancelSubscriptionStatus : new Status((int)eResponseStatus.Error, "Failed while canceling renewal");                        
                    }  
                }
            }

            return response;
        }

        private static Status Downgrade(BaseConditionalAccess cas, int groupId, string userId, long domainId, double price, string currencyCode, int productId, string couponCode, string userIp, string udid,
                                        int paymentGatewayId, int paymentMethodId, string adapterData, Subscription subscriptionInTheSameSet, DomainSubscriptionPurchaseDetails previousSubsriptionPurchaseDetails)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            // cancel existing subscription renewal
            Status cancelRenewalStatus = cas.CancelSubscriptionRenewal((int)domainId, subscriptionInTheSameSet.m_sObjectCode);

            if (cancelRenewalStatus == null || cancelRenewalStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Failed cas.CancelSubscriptionRenewal for domainId: {0}, subscriptionCode: {1}", domainId, subscriptionInTheSameSet.m_sObjectCode);
                response = cancelRenewalStatus != null ? cancelRenewalStatus : new Status((int)eResponseStatus.Error, "Failed while canceling renewal");
                return response;
            }

            long subscriptionSetModifyDetailsId = Utils.InsertSubscriptionSetModifyDetails(groupId, domainId, previousSubsriptionPurchaseDetails.PurchaseId, productId, SubscriptionSetModifyType.Downgrade);

            if (subscriptionSetModifyDetailsId <= 0)
            {
                log.ErrorFormat("Failed to insert subscription set modify details, groupId: {0}, domainId: {1}, previousSubsriptionPurchaseDetails.PurchaseId: {2}, productId: {3}, type: {4}",
                                groupId, domainId, previousSubsriptionPurchaseDetails.PurchaseId, productId, SubscriptionSetModifyType.Downgrade.ToString());
                return response;
            }

            SubscriptionSetDowngradeDetails subscriptionSetDowngradeDetails = new SubscriptionSetDowngradeDetails(subscriptionSetModifyDetailsId, groupId, userId, domainId, productId,
                                                                                                                    previousSubsriptionPurchaseDetails.sBundleCode, udid, userIp, price,
                                                                                                                    currencyCode, couponCode, paymentGatewayId, paymentMethodId, adapterData,
                                                                                                                    previousSubsriptionPurchaseDetails.dtEndDate);
            if (!Utils.InsertSubscriptionSetDowngradeDetails(subscriptionSetDowngradeDetails))
            {
                log.ErrorFormat("Failed to insert subscription set downgrade details, groupId: {0}, domainId: {1}, subscriptionSetModifyDetailsId: {2}", groupId, domainId, subscriptionSetModifyDetailsId);
                return response;
            }

            // enqueue scheduled purchase transaction
            GenericCeleryQueue queue = new GenericCeleryQueue();
            RenewTransactionData data = new RenewTransactionData(groupId, userId, subscriptionSetModifyDetailsId, string.Empty, 0,
                                                                 previousSubsriptionPurchaseDetails.dtEndDate.AddHours(-6), eSubscriptionRenewRequestType.Downgrade);
            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of subscription set downgrade scheduled purchase {0}", data);
                response = new Status((int)eResponseStatus.Error, "Failed to enqueue subscription set downgrade scheduled purchase");
            }
            else
            {
                log.DebugFormat("scheduled purchase successfully queued. data: {0}", data);
                response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public static bool HandleDowngrade(BaseConditionalAccess cas, int groupId, string userId, long subscriptionSetModifyDetailsId, ref bool shouldResetModifyStatus)
        {
            try
            {
                if (!ConditionalAccessDAL.ValidateSubscriptionSetModifyDetailsById(subscriptionSetModifyDetailsId))
                {
                    log.ErrorFormat("Downgrade details is no longer valid, groupId: {0}, userId: {1}, subscriptionSetModifyDetailsId: {2}", groupId, userId, subscriptionSetModifyDetailsId);
                    // if modify subscription set is no longer valid, return true to stop remote task from retrying                    
                    return true;
                }

                SubscriptionSetDowngradeDetails downgradeDetails = Utils.GetSubscriptionSetDowngradeDetails(groupId, subscriptionSetModifyDetailsId);
                if (downgradeDetails == null)
                {
                    shouldResetModifyStatus = true;
                    log.ErrorFormat("Downgrade details is not found on CB, groupId: {0}, userId: {1}, subscriptionSetModifyDetailsId: {2}", groupId, userId, subscriptionSetModifyDetailsId);                    
                    return false;
                }

                TransactionResponse purchaseResponse = Purchase(cas, downgradeDetails.GroupId, downgradeDetails.UserId, downgradeDetails.DomainId, downgradeDetails.Price, downgradeDetails.CurrencyCode, 0,
                                                                (int)downgradeDetails.SubscriptionId, eTransactionType.Subscription, downgradeDetails.CouponCode, downgradeDetails.UserIp, downgradeDetails.UDID,
                                                                downgradeDetails.PaymentGatewayId, downgradeDetails.PaymentMethodId, downgradeDetails.AdapterData, true);
                DateTime nextAttempt = DateTime.UtcNow.AddMinutes(55);
                if (purchaseResponse != null && purchaseResponse.Status.Code == (int)eResponseStatus.OK && purchaseResponse.State != eTransactionState.Failed.ToString())
                {
                    log.DebugFormat("Downgrade completed successfully, groupId: {0}, userId: {1}, subscriptionSetModifyDetailsId: {2}, purchaseId: {3}, transactionId: {4}",
                                    groupId, userId, subscriptionSetModifyDetailsId, purchaseResponse.Id, purchaseResponse.TransactionID);
                    if (!ConditionalAccessDAL.UpdateSubscriptionSetModifyDetails(subscriptionSetModifyDetailsId, null, 3))
                    {
                        log.ErrorFormat("Failed to Update SubscriptionSetModifyDetails to completed, groupId: {0}, userId: {1}, subscriptionSetModifyDetailsId: {2}",
                                        groupId, userId, subscriptionSetModifyDetailsId);
                    }

                    // update old subscription with end_date = now                
                    bool result = DAL.ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(downgradeDetails.UserId, downgradeDetails.PreviousSubscriptionId,
                                                                                                (int)downgradeDetails.DomainId, (int)SubscriptionPurchaseStatus.Switched);
                    if (!result)
                    {
                        log.ErrorFormat("Failed CancelSubscriptionPurchaseTransaction after Downgrade, groupId: {0}, domainId: {1}, subscriptionSetModifyDetailsId: {2}",
                                        groupId, downgradeDetails.DomainId, subscriptionSetModifyDetailsId);
                    }

                    return true;
                }
                else if (nextAttempt <= downgradeDetails.StartDate)
                {
                    // enqueue scheduled purchase transaction
                    GenericCeleryQueue queue = new GenericCeleryQueue();
                    RenewTransactionData data = new RenewTransactionData(groupId, downgradeDetails.UserId, subscriptionSetModifyDetailsId, string.Empty, 0, 
                        nextAttempt, eSubscriptionRenewRequestType.Downgrade);
                    bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                    if (!enqueueSuccessful)
                    {
                        log.ErrorFormat("Failed enqueue of subscription set downgrade scheduled purchase retry, {0}", data);
                    }
                    else
                    {
                        log.DebugFormat("scheduled purchase retry successfully queued. data: {0}", data);                        
                    }

                    shouldResetModifyStatus = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed HandleDowngrade, groupId: {0}, userId: {1}, subscriptionSetModifyDetailsId: {2}", groupId, userId, subscriptionSetModifyDetailsId), ex);
            }

            return false;
        }

        private static bool IsEntitlementContainSameSetSubscription(int groupId, long domainId, int subscriptionId, long setId, ref Subscription subscriptionInTheSameSet,
                                                                     ref DomainSubscriptionPurchaseDetails previousSubsriptionPurchaseDetails)
        {
            bool res = false;
            DomainEntitlements domainEntitlements = null;
            try
            {
                if (Utils.TryGetDomainEntitlementsFromCache(groupId, (int)domainId, null, ref domainEntitlements))
                {
                    subscriptionInTheSameSet = domainEntitlements.DomainBundleEntitlements.SubscriptionsData.Where(x => x.Value.SubscriptionSetIdsToPriority != null
                                                && x.Value.GetSubscriptionSetIdsToPriority().ContainsKey(setId) && x.Key != subscriptionId).Select(x => x.Value).FirstOrDefault();
                    if (subscriptionInTheSameSet != null)
                    {
                        res = Utils.GetPreviousSubscriptionPurchaseDetails(groupId, domainId, domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions[subscriptionInTheSameSet.m_sObjectCode],
                                                                            SubscriptionSetModifyType.Downgrade, ref previousSubsriptionPurchaseDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsEntitlementContainSameSetSubscription, groupId: {0}, domainId: {1}, subscriptionId: {2}, setId: {3}", groupId, domainId, subscriptionId, setId), ex);
            }

            return res;
        }

        private static bool CalculateUpgradePrice(BaseConditionalAccess cas, long domainId, Subscription oldSubscription, ref Price price,
                                                        DomainSubscriptionPurchaseDetails oldSubscriptionPurchaseDetails)
        {
            bool res = false;
            try
            {
                double daysLeftOnOldSubscription = Math.Floor((oldSubscriptionPurchaseDetails.dtEndDate - DateTime.UtcNow).TotalDays);

                int usageModuleLifeCycle = 0;
                if (oldSubscription.m_MultiSubscriptionUsageModule != null && oldSubscription.m_MultiSubscriptionUsageModule.Length > 0
                    && oldSubscription.m_MultiSubscriptionUsageModule[0] != null)
                {
                    usageModuleLifeCycle = oldSubscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle;
                }
                else if (oldSubscription.m_oUsageModule != null)
                {
                    usageModuleLifeCycle = oldSubscription.m_oUsageModule.m_tsMaxUsageModuleLifeCycle;
                }
                else
                {
                    log.ErrorFormat("oldSubscription Usage Module, Price Code or Price is null, domainId: {0}, oldSubscriptionCode: {1}", domainId, oldSubscription.m_sObjectCode);
                    return res;
                }

                DateTime oldStartDate = Utils.GetEndDateTime(oldSubscriptionPurchaseDetails.dtEndDate, usageModuleLifeCycle, false);
                double oldSubTotalDays = (oldSubscriptionPurchaseDetails.dtEndDate - oldStartDate).TotalDays;
                double oldSubscriptionRelativePriceToDeduct = (daysLeftOnOldSubscription / oldSubTotalDays) * oldSubscriptionPurchaseDetails.Price;
                price.m_dPrice = price.m_dPrice - oldSubscriptionRelativePriceToDeduct;
                if (price.m_dPrice < 0)
                {
                    price.m_dPrice = 0;
                }
                else
                {
                    // leave only 2 decimals after the dot
                    price.m_dPrice = Math.Round(price.m_dPrice, 2);
                }

                log.DebugFormat("daysLeftOnOldSubscription = {0}, oldSubscriptionRelativePriceTodeduct = {1}, oldSubscriptionPurchaseDetails.Price = {2}, price.m_dPrice = {3}, oldStartDate = {4}, oldSubTotalDays = {5}",
                   daysLeftOnOldSubscription, oldSubscriptionRelativePriceToDeduct, oldSubscriptionPurchaseDetails.Price, price.m_dPrice, oldStartDate, oldSubTotalDays);

                res = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(string.Format("Failed CalculateUpgradePrice, domainId: {0}, oldSubscriptionCode: {1}", domainId, oldSubscription.m_ProductCode), ex);
                res = false;
            }

            return res;
        }


        /// <summary>
        /// Purchase
        /// </summary>
        public static TransactionResponse Purchase(BaseConditionalAccess cas, int groupId, string siteguid, long household, double price,
            string currency, int contentId, int productId, eTransactionType transactionType, string coupon, string userIp, string deviceName,
            int paymentGwId, int paymentMethodId, string adapterData, bool shouldIgnoreSubscriptionSetValidation = false)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, contentId {4}, " +
                "productId {5}, productType {6}, coupon {7}, userIp {8}, deviceName {9}, paymentGwId {10}, paymentMethodId {11}, adapterData {12}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                household,                                                     // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                contentId,                                                     // {4}
                productId,                                                     // {5}   
                transactionType.ToString(),                                    // {6}
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {7}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {8}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {9}
                paymentGwId, paymentMethodId, adapterData);                    // {10,11,12}

            log.Debug(logString);

            // validate siteguid
            if (string.IsNullOrEmpty(siteguid))
            {
                response.Status.Message = "Illegal user ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            // validate currency
            if (string.IsNullOrEmpty(currency))
            {
                response.Status.Message = "Illegal currency";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            // validate productId
            if (productId < 1)
            {
                response.Status.Message = "Illegal product ID";
                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                return response;
            }

            try
            {

                // check purchase permissions 
                RolePermissions rolePermission = transactionType == eTransactionType.PPV || transactionType == eTransactionType.Collection ? RolePermissions.PURCHASE_PPV : RolePermissions.PURCHASE_SUBSCRIPTION;
                if (!APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, siteguid, rolePermission))
                {
                    response.Status = APILogic.Api.Managers.RolesPermissionsManager.GetSuspentionStatus(groupId, (int)household);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }                
                
                // validate user           
                ResponseStatus userValidStatus = ResponseStatus.OK;
                Core.Users.User user;
                userValidStatus = Utils.ValidateUser(groupId, siteguid, ref household, out user, true);

                if (userValidStatus != ResponseStatus.OK || user == null || user.m_oBasicData == null)
                {
                    // user validation failed
                    response.Status = Utils.SetResponseStatus(userValidStatus);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate household
                if (household < 1)
                {
                    response.Status.Message = "Illegal household";
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                CouponData couponData = null;

                // coupon validation
                if (!string.IsNullOrEmpty(coupon))
                {
                    couponData = Utils.GetCouponData(groupId, coupon);

                    if (couponData == null)
                    {
                        response.Status.Message = "Coupon Not Valid";
                        response.Status.Code = (int)eResponseStatus.CouponNotValid;
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        return response;
                    }
                }

                // get payment gateway
                PaymentGatewayItemResponse paymentGatewayResponse = Core.Billing.Module.GetPaymentGateway(groupId, household, paymentGwId, userIp);
                //if (paymentGatewayResponse.Status.Code != (int)eResponseStatus.OK)
                //{
                //    response.Status.Message = paymentGatewayResponse.Status.Message;
                //    response.Status.Code = paymentGatewayResponse.Status.Code;
                //    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                //    return response;
                //}

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        response = PurchasePPV(cas, groupId, siteguid, household, price, currency, contentId,
                            productId, couponData, userIp, deviceName, paymentGwId, paymentMethodId, adapterData, paymentGatewayResponse.PaymentGateway);
                        break;
                    case eTransactionType.Subscription:
                        response = PurchaseSubscription(cas, groupId, siteguid, household, price, currency,
                            productId, couponData, userIp, deviceName, paymentGwId, paymentMethodId, adapterData, shouldIgnoreSubscriptionSetValidation, user, paymentGatewayResponse.PaymentGateway);
                        break;
                    case eTransactionType.Collection:
                        response = PurchaseCollection(cas, groupId, siteguid, household, price, currency,
                            productId, coupon, userIp, deviceName, paymentGwId, paymentMethodId, adapterData, paymentGatewayResponse.PaymentGateway);
                        break;
                    default:
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal product ID");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        break;
                }

                if (response != null && response.Status != null && response.Status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetPurchaseInvalidationKey(household);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on Purchase key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Purchase Error. data: {0}", logString, ex));
            }

            return response;
        }

       
        private static TransactionResponse PurchaseCollection(BaseConditionalAccess cas, int groupId, string siteguid,
            long householdId, double price, string currency, int productId, string coupon,
            string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData, PaymentGateway paymentGateway)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, " +
                "userIp {6}, deviceName {7}, paymentGwId {8}, paymentMethodId {9} adapterData {10}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                householdId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                productId,                                                     // {4}   
                !string.IsNullOrEmpty(coupon) ? coupon : string.Empty,         // {5}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {6}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {7}
                paymentGwId, paymentMethodId, adapterData);                    // {8,9,10}

            try
            {
                string country = string.Empty;
                if (!string.IsNullOrEmpty(userIp))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = null;
                Collection collection = null;
                priceResponse = Utils.GetCollectionFinalPrice(groupId, productId.ToString(), siteguid, coupon, ref priceReason,
                                                              ref collection, country, string.Empty, deviceName, string.Empty, userIp, currency);

                if (priceReason == PriceReason.ForPurchase)
                {
                    // item is for purchase
                    if ( (priceResponse != null && priceResponse.m_dPrice == price && priceResponse.m_oCurrency.m_sCurrencyCD3 == currency) ||
                        (paymentGateway != null && paymentGateway.ExternalVerification) )
                    {
                        // price validated, create the Custom Data
                        string customData = cas.GetCustomDataForCollection(collection, productId.ToString(), siteguid, price, currency, coupon,
                                                                       userIp, country, string.Empty, deviceName, string.Empty);

                        // create new GUID for billing_transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(cas, groupId, siteguid, householdId, price, currency, userIp, customData, productId,
                            eTransactionType.Collection, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);

                        if (response != null && response.Status != null)
                        {
                            // Status OK + (State OK || State Pending) = grant entitlement
                            if (response.Status.Code == (int)eResponseStatus.OK &&
                               (response.State.Equals(eTransactionState.OK.ToString()) ||
                                response.State.Equals(eTransactionState.Pending.ToString())))
                            {
                                // purchase passed, update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                response.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                                // grant entitlement
                                long purchaseID = 0;
                                bool handleBillingPassed =
                                    cas.HandleCollectionBillingSuccess(ref response, siteguid, householdId, collection, price, currency, coupon, userIp,
                                        country, deviceName, long.Parse(response.TransactionID), customData, productId,
                                        billingGuid, false, entitlementDate, ref purchaseID);

                                if (handleBillingPassed)
                                {
                                    cas.WriteToUserLog(siteguid, string.Format("Collection Purchase, ProductID:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                        productId, purchaseID, response.TransactionID));

                                    // entitlement passed - build notification message
                                    var dicData = new Dictionary<string, object>()
                                {
                                    {"CollectionCode", productId},
                                    {"BillingTransactionID", response.TransactionID},
                                    {"SiteGUID", siteguid},
                                    {"PurchaseID", purchaseID},
                                    {"CouponCode", coupon},
                                    {"CustomData", customData}
                                };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData))
                                    {
                                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }
                                }
                                else
                                {
                                    // purchase passed, entitlement failed
                                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed but entitlement failed");
                                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase failed - received error status
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed - no status error
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The price of the request is not the actual price");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    response.Status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
            }
            return response;
        }

        private static TransactionResponse PurchaseSubscription(BaseConditionalAccess cas, int groupId, string siteguid,
            long householdId, double price, string currency, int productId, CouponData coupon, string userIp, string deviceName,
            int paymentGwId, int paymentMethodId, string adapterData, bool isSubscriptionSetModifySubscription,
            Core.Users.User user, PaymentGateway paymentGateway)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;               
            }        

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, " +
                "userIp {6}, deviceName {7}, " +
                "paymentGwId {8}, paymentMethodId {9}, adapterData {10}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                householdId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                productId,                                                     // {4}   
                !string.IsNullOrEmpty(couponCode) ? couponCode : string.Empty, // {5}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {6}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {7}
                paymentGwId, paymentMethodId, adapterData);

            try
            {
                string country = string.Empty;

                if (!string.IsNullOrEmpty(userIp))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription subscription = null;

                bool isGiftCard = false;
                Price priceResponse = null;

                UnifiedBillingCycle unifiedBillingCycle = null; // there is a unified billingCycle for this subscription cycle
                priceResponse = Utils.GetSubscriptionFinalPrice(groupId, productId.ToString(), siteguid, ref couponCode,
                    ref priceReason, ref subscription, country, string.Empty, deviceName, userIp, ref unifiedBillingCycle, currency, isSubscriptionSetModifySubscription);

                if (subscription == null)
                {
                    response.Status.Message = "ProductId doesn't exist";
                    return response;
                }

                // check permission for subscription with premuim services - by roles 
                if (subscription.m_lServices != null && subscription.m_lServices.Count() > 0)
                {
                    if (!APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, siteguid, RolePermissions.PURCHASE_SERVICE))
                    {
                        response.Status = APILogic.Api.Managers.RolesPermissionsManager.GetSuspentionStatus(groupId, (int)householdId);
                        log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                        return response;
                    }
                }
                
                // if unified billing cycle is in the "history" ignore it in purchase ! 
                if (unifiedBillingCycle != null && unifiedBillingCycle.endDate < ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(DateTime.UtcNow))
                {
                    unifiedBillingCycle = null;
                }

                // if the payment gateway is external ignore unified billing !!
                if (paymentGateway != null && paymentGateway.ExternalVerification)
                {
                    log.DebugFormat("paymentGateway.ExternalVerification {0}", paymentGateway.ID);
                    unifiedBillingCycle = null;
                }

                if (subscription.m_UserTypes != null && subscription.m_UserTypes.Length > 0 && !subscription.m_UserTypes.Contains(user.m_oBasicData.m_UserType))
                {
                    response.Status = new Status((int)eResponseStatus.SubscriptionNotAllowedForUserType, eResponseStatus.SubscriptionNotAllowedForUserType.ToString());
                    return response;
                }

                if (!isSubscriptionSetModifySubscription && subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
                {
                    if (subscription.Type == SubscriptionType.AddOn)
                    {
                        ApiObjects.Response.Status Status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                        if (Status.Code != (int)eResponseStatus.OK)
                        {
                            response.Status = Status;
                            return response;
                        }
                    }
                    else
                    {
                        List<SubscriptionSet> groupSwitchSubscriptionSets = Pricing.Utils.GetSubscriptionSets(groupId, null, SubscriptionSetType.Switch);
                        HashSet<long> switchSubscriptionSetsIds = groupSwitchSubscriptionSets != null && groupSwitchSubscriptionSets.Count > 0 ? 
                                                                new HashSet<long>(groupSwitchSubscriptionSets.Select(x => x.Id).ToList()) : new HashSet<long>();
                        if (switchSubscriptionSetsIds.Count > 0)
                        {
                            foreach (KeyValuePair<long, int> setAndPriority in subscription.GetSubscriptionSetIdsToPriority())
                            {
                                if (switchSubscriptionSetsIds.Contains(setAndPriority.Key))
                                {
                                    Subscription subscriptionInTheSameSet = null;
                                    DomainSubscriptionPurchaseDetails oldSubscriptionPurchaseDetails = null;
                                    if (IsEntitlementContainSameSetSubscription(groupId, householdId, productId, setAndPriority.Key, ref subscriptionInTheSameSet, ref oldSubscriptionPurchaseDetails))
                                    {
                                        response.Status = new Status((int)eResponseStatus.CanOnlyBeEntitledToOneSubscriptionPerSubscriptionSet,
                                                                    "Can only be entitled to one subscription per subscriptionSet, please use Upgrade or Downgrade");
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }

                if (coupon != null && coupon.m_CouponStatus == CouponsStatus.Valid && coupon.m_oCouponGroup != null && 
                    coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard &&
                    ((subscription.m_oCouponsGroup != null && subscription.m_oCouponsGroup.m_sGroupCode == coupon.m_oCouponGroup.m_sGroupCode) ||
                     (subscription.CouponsGroups != null && subscription.CouponsGroups.Count() > 0 && 
                     subscription.CouponsGroups.Where(x => x.m_sGroupCode == coupon.m_oCouponGroup.m_sGroupCode).Count() > 0 )))
                {
                    isGiftCard = true;
                    priceResponse = new Price()
                    {
                        m_dPrice = 0.0,
                        m_oCurrency = new Currency()
                        {
                            m_sCurrencyCD3 = currency
                        }
                    };
                }

                bool entitleToPreview = priceReason == PriceReason.EntitledToPreviewModule;
                bool couponFullDiscount = (priceReason == PriceReason.Free && coupon != null);

                if ((priceReason == PriceReason.ForPurchase ||
                    entitleToPreview ||
                    couponFullDiscount) ||
                    (isGiftCard && (priceReason == PriceReason.ForPurchase || priceReason == PriceReason.Free)))
                {
                    // item is for purchase
                    if (isSubscriptionSetModifySubscription || 
                        (priceResponse != null && priceResponse.m_dPrice == price && priceResponse.m_oCurrency.m_sCurrencyCD3 == currency) || 
                        (paymentGateway != null && paymentGateway.ExternalVerification))
                    {
                        // price is validated, create custom data
                        bool partialPrice = unifiedBillingCycle != null && unifiedBillingCycle.endDate > 0 && unifiedBillingCycle.endDate > ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(DateTime.UtcNow);
                        string customData = cas.GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, siteguid, price, currency,
                                                                         couponCode, userIp, country, string.Empty, deviceName, string.Empty,
                                                                         entitleToPreview ? subscription.m_oPreviewModule.m_nID + "" : string.Empty,
                                                                         entitleToPreview, false, 0, false, null, partialPrice);                        

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        if (couponFullDiscount || isGiftCard)
                        {
                            response = HandleFullCouponPurchase(cas, groupId, siteguid, price, currency, userIp, 
                                customData, productId, eTransactionType.Subscription, billingGuid, 0, isGiftCard);
                        }
                        else
                        {   
                            response = HandlePurchase(cas, groupId, siteguid, householdId, price, currency, userIp, customData, productId,
                                                      eTransactionType.Subscription, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);
                        }
                        if (response != null && response.Status != null)
                        {
                            // Status OK + (State OK || State Pending) = grant entitlement
                            if (response.Status.Code == (int)eResponseStatus.OK &&
                               (response.State.Equals(eTransactionState.OK.ToString()) ||
                                response.State.Equals(eTransactionState.Pending.ToString())))
                            {
                                // purchase passed
                                long purchaseID = 0;

                                if (paymentGateway == null)
                                {
                                    paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, householdId, billingGuid);
                                }

                                // update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                DateTime? endDate = null;
                                response.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                                if (isGiftCard)
                                {
                                    endDate = CalculateGiftCardEndDate(cas, coupon, subscription, entitlementDate);
                                }

                                if (unifiedBillingCycle != null && !entitleToPreview && string.IsNullOrEmpty(couponCode))   
                                {
                                    // calculate end date by unified billing cycle
                                    endDate = ODBCWrapper.Utils.UnixTimestampToDateTimeMilliseconds(unifiedBillingCycle.endDate);
                                }

                                //try get from db process_purchases_id - if not exsits - create one - only if pare of billing cycle
                                long processId = 0;
                                bool isNew = false;
                                if (paymentGateway != null)
                                {
                                    if (!endDate.HasValue)
                                    {
                                        endDate = Utils.CalcSubscriptionEndDate(subscription, entitleToPreview, entitlementDate);
                                    }

                                    //create process id only for subscription that equal the cycle and are NOT preview module or purchase with coupon
                                    long ? groupUnifiedBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
                                    if (!paymentGateway.ExternalVerification && 
                                        subscription != null && subscription.m_bIsRecurring && subscription.m_MultiSubscriptionUsageModule != null &&
                                         subscription.m_MultiSubscriptionUsageModule.Count() == 1 /*only one price plan*/
                                         && groupUnifiedBillingCycle.HasValue
                                         && (int)groupUnifiedBillingCycle.Value == subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle
                                        )
                                    // group define with billing cycle
                                    {
                                        if (unifiedBillingCycle == null || (!entitleToPreview && string.IsNullOrEmpty(couponCode)))
                                        {
                                            processId = Utils.GetUnifiedProcessId(groupId, paymentGateway.ID, endDate.Value, householdId, out isNew);
                                        }
                                    }
                                }

                                // grant entitlement
                                bool handleBillingPassed = 
                                    cas.HandleSubscriptionBillingSuccess(ref response, siteguid, householdId, subscription, price, currency, couponCode, 
                                        userIp, country, deviceName, long.Parse(response.TransactionID), customData, productId, billingGuid.ToString(),
                                        entitleToPreview, subscription.m_bIsRecurring, entitlementDate, ref purchaseID, ref endDate, SubscriptionPurchaseStatus.OK, processId);

                                if (handleBillingPassed && endDate.HasValue)
                                {
                                    cas.WriteToUserLog(siteguid, string.Format("Subscription Purchase, productId:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                        productId, purchaseID, response.TransactionID));

                                    // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                                    if (subscription.m_nDomainLimitationModule != 0)
                                    {
                                        cas.UpdateDLM(householdId, subscription.m_nDomainLimitationModule);
                                    }

                                    long endDateUnix = 0;

                                    if (endDate.HasValue)
                                    {
                                        endDateUnix = TVinciShared.DateUtils.DateTimeToUnixTimestamp((DateTime)endDate);
                                    }

                                    // If the subscription if recurring, put a message for renewal and all that...
                                    if (subscription.m_bIsRecurring)
                                    {
                                        DateTime nextRenewalDate = endDate.Value;

                                        if (!isGiftCard)
                                        {
                                            // call billing process renewal
                                            try
                                            {
                                                nextRenewalDate = endDate.Value.AddMinutes(-5);

                                                if (paymentGateway == null)
                                                {
                                                    // error getting PG
                                                    log.Error("Error getting the PG - GetPaymentGatewayByBillingGuid");
                                                }
                                                else
                                                {
                                                    nextRenewalDate = endDate.Value.AddMinutes(paymentGateway.RenewalStartMinutes);
                                                    paymentGwId = paymentGateway.ID;

                                                    if (!paymentGateway.ExternalVerification && unifiedBillingCycle == null && subscription != null &&
                                                     subscription.m_bIsRecurring && subscription.m_MultiSubscriptionUsageModule != null &&
                                                     subscription.m_MultiSubscriptionUsageModule.Count() == 1)
                                                    {
                                                        Utils.HandleDomainUnifiedBillingCycle(groupId, householdId, ref unifiedBillingCycle, subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle, endDate.Value, !string.IsNullOrEmpty(couponCode));
                                                    }
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error("Error while trying to get the PG", ex);
                                            }
                                        }
                                        else
                                        {
                                            PurchaseManager.SendGiftCardReminderEmails(cas, groupId, endDateUnix, nextRenewalDate, siteguid, householdId, purchaseID, billingGuid);
                                        }

                                        // enqueue renew transaction
                                        #region Renew transaction message in queue
                                        /*
                                         check from configuration if unified cycle
                                            if      message not exists in queue – add new message 
                                            else    message exists yet – do nothing
                                        create new queue with new messages for each Payment Gateway 
                                        */
                                        if (isNew) // need to insert new unified billing message to queue
                                        {
                                            Utils.RenewUnifiedTransactionMessageInQueue(groupId, householdId,
                                                ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(endDate.Value), nextRenewalDate, processId);
                                        }
                                        else if (unifiedBillingCycle == null || ((entitleToPreview || !string.IsNullOrEmpty(couponCode)) && !isNew))
                                        {
                                            // insert regular message 
                                            RenewTransactionMessageInQueue(groupId, siteguid, billingGuid, purchaseID, endDateUnix, nextRenewalDate, householdId);
                                        }

                                        //else do nothing, message already exists

                                        #endregion
                                    }
                                    else
                                    // If subscription is not recurring, enqueue subscription ends message
                                    {
                                        RenewManager.EnqueueSubscriptionEndsMessage(groupId, siteguid, purchaseID, endDateUnix);
                                    }

                                    // build notification message
                                    var dicData = new Dictionary<string, object>()
                                    {
                                        {"SubscriptionCode", productId},
                                        {"BillingTransactionID", response.TransactionID},
                                        {"SiteGUID", siteguid},
                                        {"PurchaseID", purchaseID},
                                        {"CouponCode", coupon},
                                        {"CustomData", customData}
                                    };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData))
                                    {
                                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }
                                }
                                else
                                {
                                    // purchase passed, entitlement failed
                                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed but entitlement failed");
                                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase failed - received error status
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed - no status error
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The price of the request is not the actual price");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // item not for purchase
                    response.Status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        public static void SendRenewalReminder(BaseConditionalAccess cas, int groupId, DateTime endDate, long endDateUnix,
            string siteguid, long householdId, long purchaseId, string billingGuid)
        {
            int renewalReminderSettings = BillingDAL.GetRenewalReminderSettings(groupId);

            if (renewalReminderSettings > 0)
            {
                var domain = Utils.GetDomainInfo((int)householdId, groupId);

                if (domain != null && domain.m_nStatus == 1 && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
                {
                    string masterSiteGuid = domain.m_masterGUIDs.First().ToString();

                    RenewTransactionsQueue queue = new RenewTransactionsQueue();

                    DateTime eta = endDate.AddDays(-1 * renewalReminderSettings);

                    if (eta > DateTime.UtcNow)
                    {
                        RenewTransactionData data = new RenewTransactionData(groupId, masterSiteGuid, purchaseId, billingGuid,
                            endDateUnix, eta, eSubscriptionRenewRequestType.RenewalReminder);
                        bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                        if (!enqueueSuccessful)
                        {
                            log.ErrorFormat("Failed enqueue of reminder transaction {0}", data);
                        }
                        else
                        {
                            log.DebugFormat("New task created - normal renewal reminder. next reminder date: {0}, data: {1}", eta, data);
                        }
                    }
                }
            }
        }

        public static void SendRenewalReminder(RenewTransactionData data, long householdId)
        {
            try
            {
                if (data == null || data.ETA == null || !data.ETA.HasValue)
                {
                    return;
                }

                int renewalReminderSettings = BillingDAL.GetRenewalReminderSettings(data.GroupId);

                if (renewalReminderSettings > 0)
                {
                    Users.Domain domain = null;

                    if (householdId == 0)
                    {
                        var domainResponse = Core.Domains.Module.GetDomainByUser(data.GroupId, data.siteGuid);

                        if (domainResponse != null && domainResponse.Status != null && domainResponse.Status.Code == (int)eResponseStatus.OK)
                        {
                            domain = domainResponse.Domain;
                        }
                    }
                    else
                    {
                        domain = Utils.GetDomainInfo((int)householdId, data.GroupId);
                    }

                    if (domain != null && domain.m_nStatus == 1 && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
                    {
                        string masterSiteGuid = domain.m_masterGUIDs.First().ToString();

                        RenewTransactionsQueue queue = new RenewTransactionsQueue();

                        DateTime eta = data.ETA.Value.AddDays(-1 * renewalReminderSettings);

                        if (eta > DateTime.UtcNow)
                        {
                            RenewTransactionData newData = new RenewTransactionData(data.GroupId, masterSiteGuid, data.purchaseId, data.billingGuid,
                                data.endDate, eta, eSubscriptionRenewRequestType.RenewalReminder);
                            bool enqueueSuccessful = queue.Enqueue(newData, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, data.GroupId));

                            if (!enqueueSuccessful)
                            {
                                log.ErrorFormat("Failed enqueue of reminder transaction {0}", data);
                            }
                            else
                            {
                                log.DebugFormat("New task created - normal renewal reminder. next reminder date: {0}, data: {1}", eta, newData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on SendRenewalReminder, ex = {0}", ex);
            }
        }

        public static void SendRenewalReminder(RenewUnifiedData data)
        {
            try
            {
                if (data == null || data.ETA == null || !data.ETA.HasValue)
                {
                    return;
                }

                int renewalReminderSettings = BillingDAL.GetRenewalReminderSettings(data.GroupId);

                if (renewalReminderSettings > 0)
                {
                    RenewTransactionsQueue queue = new RenewTransactionsQueue();

                    DateTime eta = data.ETA.Value.AddDays(-1 * renewalReminderSettings);

                    if (eta > DateTime.UtcNow)
                    {
                        RenewUnifiedData newData = new RenewUnifiedData(data.GroupId, data.householdId, data.processId,
                            data.endDate, eta, eSubscriptionRenewRequestType.RenewalReminder);
                        bool enqueueSuccessful = queue.Enqueue(newData, string.Format(Utils.ROUTING_KEY_PROCESS_UNIFIED_RENEW_SUBSCRIPTION, data.GroupId));

                        if (!enqueueSuccessful)
                        {
                            log.ErrorFormat("Failed enqueue of reminder transaction {0}", data);
                        }
                        else
                        {
                            log.DebugFormat("New task created - normal renewal reminder. next reminder date: {0}, data: {1}", eta, newData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on SendRenewalReminder, ex = {0}", ex);
            }
        }

        private static bool RenewTransactionMessageInQueue(int groupId, string siteguid, string billingGuid, 
            long purchaseID, long endDateUnix, DateTime nextRenewalDate, long householdId = 0)
        {
            log.DebugFormat("RenewTransactionMessageInQueue (RenewTransactionData) purchaseId:{0}", purchaseID);

            RenewTransactionsQueue queue = new RenewTransactionsQueue();
            RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseID, billingGuid, endDateUnix, nextRenewalDate);
            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
            }
            else
            {
                PurchaseManager.SendRenewalReminder(data, householdId);
                log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}",
                    nextRenewalDate, data);
            }

            return enqueueSuccessful;
        }

        ///If needed create/ update doc in cb for unifiedBilling_household_{ household_id }_renewBillingCycle
        ///create: unified billing cycle for household (CB)
        ///update: the current one with payment gateway id or end date 
        private static void HandleDomainUnifiedBillingCycle(int groupId, long householdId, long subscriptionBillingCycle, UnifiedBillingCycle unifiedBillingCycle, DateTime endDate)//, int paymentGatewayId, long currentDate)
        {            
            try
            {
                long? groupUnifiedBillingCycle = Utils.GetGroupUnifiedBillingCycle(groupId);
              
                if (groupUnifiedBillingCycle.HasValue) // group define with billing cycle
                {
                   long nextEndDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtcMilliseconds(endDate);
                   if (unifiedBillingCycle != null && unifiedBillingCycle.endDate != nextEndDate)
                    {
                        // update unified billing by endDate or paymentGatewatId                  
                        bool setResult = UnifiedBillingCycleManager.SetDomainUnifiedBillingCycle(householdId, groupUnifiedBillingCycle.Value, nextEndDate); //, paymentGWIds);
                    }                   
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("HandleDomainUnifiedBillingCycle failed with ex = {0}", ex);
            }
        }

        private static void SendGiftCardReminderEmails(BaseConditionalAccess cas, int groupId, long endDate, DateTime renewDate,
            string siteGuid, long householdId, long purchaseId, string billingGuid)
        {
            List<int> remindersDays = PricingDAL.GetGiftCardReminders(groupId);

            if (remindersDays != null && remindersDays.Count > 0)
            {
                var domain = Utils.GetDomainInfo((int)householdId, groupId);

                if (domain != null && domain.m_nStatus == 1 && domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0)
                {
                    string masterSiteGuid = domain.m_masterGUIDs.First().ToString();

                    RenewTransactionsQueue queue = new RenewTransactionsQueue();

                    foreach (var reminder in remindersDays)
                    {
                        DateTime eta = renewDate.AddDays(-1 * reminder);

                        if (eta > DateTime.UtcNow)
                        {
                            RenewTransactionData data = new RenewTransactionData(groupId, masterSiteGuid, purchaseId, billingGuid,
                                endDate, eta, eSubscriptionRenewRequestType.GiftCardReminder);
                            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                            if (!enqueueSuccessful)
                            {
                                log.ErrorFormat("Failed enqueue of reminder transaction {0}", data);
                            }
                            else
                            {
                                log.DebugFormat("New task created (upon Gift card subscription purchase success). next reminder date: {0}, data: {1}", eta, data);
                            }
                        }
                    }
                }
            }
        }

        private static DateTime? CalculateGiftCardEndDate(BaseConditionalAccess cas, CouponData coupon, Subscription subscription, DateTime entitlementDate)
        {
            // Calculate first end date with normal rules
            var initialEndDate = Utils.CalcSubscriptionEndDate(subscription, false, entitlementDate);

            // get the time span between now and the first period end date
            var timeSpan = initialEndDate - entitlementDate;

            DateTime endDate;

            if (coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon > 0)
            {
                // expand (multiply) this period by the coupon's recurring cound
                var newTimeSpan = new TimeSpan(timeSpan.Ticks * coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon);

                // new end date is now + X times the initial period
                endDate = DateTime.UtcNow + newTimeSpan;
            }
            else
            {
                // if it is not recurring, simply use first, inital end date
                endDate = initialEndDate;
            }

            return endDate;
        }


        private static TransactionResponse PurchasePPV(BaseConditionalAccess cas, int groupId, string siteguid, long householdId, double price,
            string currency, int contentId, int productId, CouponData coupon, string userIp, string deviceName, int paymentGwId,
            int paymentMethodId, string adapterData, PaymentGateway paymentGateway)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;
            }

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, contentId {4}, productId {5}, coupon {6}, " +
                "userIp {7}, deviceName {8}, paymentGwId {9}, paymentMethodId {10}, adapterData {11}",
                !string.IsNullOrEmpty(siteguid) ? siteguid : string.Empty,     // {0}
                householdId,                                                   // {1}
                price,                                                         // {2}  
                !string.IsNullOrEmpty(currency) ? currency : string.Empty,     // {3}
                contentId,                                                     // {4}
                productId,                                                     // {5}   
                !string.IsNullOrEmpty(couponCode) ? couponCode : string.Empty,         // {6}
                !string.IsNullOrEmpty(userIp) ? userIp : string.Empty,         // {7}
                !string.IsNullOrEmpty(deviceName) ? deviceName : string.Empty, // {8}
                paymentGwId, paymentMethodId, adapterData);                    // {9,10,11}

            try
            {
                // validate content ID
                if (contentId < 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, ILLEGAL_CONTENT_ID);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate content ID related media
                int mediaID = ConditionalAccess.Utils.GetMediaIDFromFileID(contentId, groupId);
                if (mediaID < 1)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Content ID with a related media");
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate PPV 
                PPVModule ppvModule = null;
                ApiObjects.Response.Status status = Utils.ValidatePPVModuleCode(groupId, productId, contentId, ref ppvModule);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = status;
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;

                bool isGiftCard = false;
                Price priceObject = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(contentId, ppvModule, siteguid, couponCode,
                        groupId, ref priceReason, ref relevantSub, ref relevantCol, ref relevantPP, string.Empty, string.Empty, deviceName,
                        false, userIp, currency, BlockEntitlementType.NO_BLOCK);

                if (coupon != null &&
                    coupon.m_CouponStatus == CouponsStatus.Valid &&
                    coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard &&
                    ppvModule != null &&
                    ppvModule.m_oCouponsGroup != null &&
                    coupon.m_oCouponGroup != null &&
                    ppvModule.m_oCouponsGroup.m_sGroupCode == coupon.m_oCouponGroup.m_sGroupCode)
                {
                    isGiftCard = true;
                }

                bool couponFullDiscount = (priceReason == PriceReason.Free) && coupon != null;

                if ((priceReason == PriceReason.ForPurchase ||
                    (priceReason == PriceReason.SubscriptionPurchased && priceObject.m_dPrice > 0) ||
                    couponFullDiscount) ||
                    (isGiftCard && (priceReason == PriceReason.ForPurchase || priceReason == PriceReason.Free)))
                {
                    if (isGiftCard)
                    {
                        priceReason = PriceReason.Free;
                        priceObject = new Price()
                        {
                            m_dPrice = 0.0,
                            m_oCurrency = new Currency()
                            {
                                m_sCurrencyCD3 = currency
                            }
                        };
                    }

                    // item is for purchase
                    if ( (priceObject.m_dPrice == price && priceObject.m_oCurrency.m_sCurrencyCD3 == currency)
                        || (paymentGateway != null && paymentGateway.ExternalVerification) )
                    {
                        string country = string.Empty;
                        if (!string.IsNullOrEmpty(userIp))
                        {
                            // get country by user IP
                            country = Utils.GetIP2CountryName(groupId, userIp);
                        }

                        // create custom data
                        string customData = cas.GetCustomData(relevantSub, ppvModule, null, siteguid, price, currency,
                                                          contentId, mediaID, productId.ToString(), string.Empty, couponCode,
                                                          userIp, country, string.Empty, deviceName);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        if (couponFullDiscount || isGiftCard)
                        {
                            response = HandleFullCouponPurchase(cas, groupId, siteguid, price, currency, userIp, customData,
                                productId, eTransactionType.PPV, billingGuid, contentId, isGiftCard);
                        }
                        else
                        {
                            response = HandlePurchase(cas, groupId, siteguid, householdId, price, currency, userIp, customData, productId,
                                eTransactionType.PPV, billingGuid, paymentGwId, contentId, paymentMethodId, adapterData);
                        }

                        if (response != null &&
                            response.Status != null)
                        {
                            // Status OK + (State OK || State Pending) = grant entitlement
                            if (response.Status.Code == (int)eResponseStatus.OK &&
                               (response.State.Equals(eTransactionState.OK.ToString()) ||
                                response.State.Equals(eTransactionState.Pending.ToString())))
                            {
                                //PurchaseManager.SendGiftCardPurchaseMail(cas, groupId, siteguid, contentId, price, currency, DateTime.UtcNow, eTransactionType.PPV);

                                // purchase passed
                                long purchaseId = 0;

                                // update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                response.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                                // grant entitlement
                                bool handleBillingPassed = cas.HandlePPVBillingSuccess(ref response, siteguid, householdId, relevantSub, price, currency, couponCode, userIp,
                                    country, deviceName, long.Parse(response.TransactionID), customData, ppvModule, productId, contentId,
                                    billingGuid, entitlementDate, ref purchaseId);

                                if (handleBillingPassed)
                                {
                                    cas.WriteToUserLog(siteguid, string.Format("PPV Purchase, ProductID:{0}, ContentID:{1}, PurchaseID:{2}, BillingTransactionID:{3}",
                                        productId, contentId, purchaseId, response.TransactionID));

                                    // entitlement passed - build notification message
                                    var dicData = new Dictionary<string, object>()
                                    {
                                        {"MediaFileID", contentId},
                                        {"BillingTransactionID", response.TransactionID},
                                        {"PPVModuleCode", productId},
                                        {"SiteGUID", siteguid},
                                        {"CouponCode", coupon},
                                        {"CustomData", customData},
                                        {"PurchaseID", purchaseId}
                                    };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData))
                                    {
                                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }
                                }
                                else
                                {
                                    // purchase passed, entitlement failed
                                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase passed, entitlement failed");
                                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase failed - received error status
                                log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                            }
                        }
                        else
                        {
                            // purchase failed - no status error
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IncorrectPrice, "The request price is incorrect");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    response.Status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
                log.Error("Exception occurred. data: " + logString, ex);
            }
            return response;
        }

        private static void SendGiftCardPurchaseMail(
            BaseConditionalAccess cas, int groupId, string siteGuid, int contentId, double price, string currency,
            DateTime purchaseDate, eTransactionType transactionType, string itemName = "")
        {
            try
            {
                string email = string.Empty;

                if (transactionType == eTransactionType.PPV)
                {
                    int[] mediaFileIds = new int[] { contentId };
                    MeidaMaper[] mapper = Utils.GetMediaMapper(groupId, mediaFileIds);

                    if (mapper != null && mapper.Length > 0)
                    {
                        int mediaID = Utils.ExtractMediaIDOutOfMediaMapper(mapper, contentId);
                        itemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", mediaID, "MAIN_CONNECTION_STRING").ToString();
                    }
                }

                string purchaseDateString = cas.GetDateSTRByGroup(purchaseDate, groupId);

                var dummyRequest =
                    cas.GetPurchaseMailRequest(ref email, siteGuid, itemName, string.Empty, purchaseDateString, string.Empty, price, currency, groupId);

                PurchaseViaGiftCardMailRequest mailRequest = new PurchaseViaGiftCardMailRequest()
                {
                    m_emailKey = dummyRequest.m_emailKey,
                    m_eMailType = dummyRequest.m_eMailType,
                    m_sBCCAddress = dummyRequest.m_sBCCAddress,
                    m_sFirstName = dummyRequest.m_sFirstName,
                    m_sLastName = dummyRequest.m_sLastName,
                    m_sSenderFrom = dummyRequest.m_sSenderFrom,
                    m_sSenderName = dummyRequest.m_sSenderName,
                    m_sSenderTo = dummyRequest.m_sSenderTo,
                    m_sSubject = dummyRequest.m_sSubject,
                    m_sTemplateName = dummyRequest.m_sTemplateName,
                    m_sItemName = itemName,
                    offerType = transactionType.ToString(),
                    m_sPurchaseDate = purchaseDateString
                };

                var response = Core.Api.Module.SendMailTemplate(groupId, mailRequest);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("PurchaseManager: Error while calling WS API GetExternalEntitlements. groupId = {0}, userId = {1}", groupId, siteGuid), ex);
            }
        }

        private static TransactionResponse HandleGiftCardPurchase()
        {
            TransactionResponse response = new TransactionResponse()
            {
                Status = new Status(),
                State = eTransactionState.OK.ToString(),
                FailReasonCode = 0,
                TransactionID = "0"
            };

            return response;
        }

        protected internal static TransactionResponse HandlePurchase(BaseConditionalAccess cas, int groupId, string siteGUID,
            long houseHoldID, double price, string currency, string userIP, string customData,
            int productID, eTransactionType transactionType, string billingGuid, int paymentGWId, int contentId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse();

            string logString = string.Format("fail get response from billing service siteGUID={0}, houseHoldID={1}, price={2}, currency={3}, " +
                "userIP={4}, customData={5}, productID={6}, " +
                "(int)transactionType={7}, billingGuid={8}, paymentGWId={9}, paymentMethodId = {10}, adapterData = {11}",
                                       !string.IsNullOrEmpty(siteGUID) ? siteGUID : string.Empty,              // {0}
                                       houseHoldID,                                                            // {1}
                                       price,                                                                  // {2}
                                       !string.IsNullOrEmpty(currency) ? currency : string.Empty,              // {3}
                                       !string.IsNullOrEmpty(userIP) ? userIP : string.Empty,                  // {4}
                                       !string.IsNullOrEmpty(customData) ? customData : string.Empty,          // {5}
                                       productID,                                                              // {6}
                                       (int)transactionType,                                                   // {7}
                                       !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty,        // {8}
                                       paymentGWId, paymentMethodId, adapterData);                             // {9,10,11}

            try
            {
                // call new billing method for charge adapter
                var transactionResponse =
                    Core.Billing.Module.Transact(groupId, siteGUID, (int)houseHoldID, price, currency,
                        userIP, customData, productID, transactionType, contentId, billingGuid, paymentGWId, paymentMethodId, adapterData);

                response = BaseConditionalAccess.ConvertTransactResultToTransactionResponse(logString, transactionResponse);
            }
            catch (Exception ex)
            {
                log.Error(logString, ex);
                response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        internal static TransactionResponse HandleFullCouponPurchase(BaseConditionalAccess cas, int groupId, string siteGUID,
            double price, string currency, string userIP, string customData, int productID, eTransactionType transactionType,
            string billingGuid, int contentId, bool isGiftCard = false)
        {
            TransactionResponse response = new TransactionResponse();

            try
            {
                string extraParams = string.Empty;

                if (isGiftCard)
                {
                    extraParams = "GIFT_CARD";
                }

                // call new billing method for charge adapter
                var transactionResponse = Core.Billing.Module.CC_DummyChargeUser(groupId, siteGUID, price, currency, userIP, customData, 1, 1, extraParams);
                long billingTransactionId = 0;
                if (transactionResponse.m_oStatus == BillingResponseStatus.Success && long.TryParse(transactionResponse.m_sRecieptCode, out billingTransactionId))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    response.State = eTransactionState.OK.ToString();
                    response.FailReasonCode = 0;
                    response.TransactionID = transactionResponse.m_sRecieptCode;

                    ApiDAL.UpdateBillingTransactionGuid(billingTransactionId, billingGuid);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error HandleGiftPurchase", ex);
                response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }        

        #endregion        
    }
}
