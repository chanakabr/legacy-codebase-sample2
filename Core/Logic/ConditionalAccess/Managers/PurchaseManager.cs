using ApiLogic.Pricing.Handlers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using ApiObjects.SubscriptionSet;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Api.Managers;
using Core.Pricing;
using Core.Pricing.Handlers;
using DAL;
using Phx.Lib.Log;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FeatureFlag;
using TVinciShared;

namespace Core.ConditionalAccess
{
    public class PurchaseManager
    {
        #region Consts

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
                response.Set((int)eResponseStatus.InvalidUser, "Illegal user ID");
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
                    couponData = Utils.GetCouponData(groupId, couponCode, domainId);

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
                Subscription subscription = Utils.GetSubscription(groupId, productId, userId);

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

            if (couponData != null && couponData.m_CouponStatus == CouponsStatus.Valid && couponData.m_oCouponGroup != null 
                && couponData.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard && 
                ((subscription.m_oCouponsGroup != null && subscription.m_oCouponsGroup.m_sGroupCode == couponData.m_oCouponGroup.m_sGroupCode) 
                || (subscription.GetValidSubscriptionCouponGroup(couponData.m_oCouponGroup.m_sGroupCode)?.Count > 0)))
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
                            transactionResponse.CreatedAt = DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlementDate);

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
                                        endDateUnix = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds((DateTime)endDate);
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

                                    var enqueueSuccessful = true;
                                    var data = new RenewTransactionData(groupId, userId, purchaseID, billingGuid, endDateUnix, nextRenewalDate);
                                    if (PhoenixFeatureFlagInstance.Get().IsRenewUseKronos())
                                    {
                                        ConditionalAccessDAL.Insert_SubscriptionsPurchasesKronos(purchaseID);
                                        
                                        log.Debug($"Kronos - Renew purchaseID:{purchaseID}");
                                        RenewManager.addEventToKronos(groupId, data);
                                    }
                                    else
                                    {
                                        var queue = new RenewTransactionsQueue();
                                        enqueueSuccessful &= queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                                        if (!enqueueSuccessful)
                                        {
                                            log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                                        }
                                        else
                                        {
                                            log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}",
                                                nextRenewalDate, data);
                                        }
                                    }

                                    if (enqueueSuccessful)
                                    {
                                        PurchaseManager.SendRenewalReminder(groupId, data, domainId);
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
                                if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData, userId, udid, userIp))
                                {
                                    log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", transactionResponse.Status.Message, logString);
                                }
                            }
                            else
                            {
                                // purchase passed, entitlement failed
                                transactionResponse.Status = new Status((int)eResponseStatus.PurchasePassedEntitlementFailed, BaseConditionalAccess.PURCHASE_PASSED_ENTITLEMENT_FAILED);
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
                        transactionResponse.Status = new Status((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                        log.ErrorFormat("Error: {0}, data: {1}", transactionResponse.Status.Message, logString);
                    }
                }
                else
                {
                    // incorrect price
                    transactionResponse.Status = new Status((int)eResponseStatus.IncorrectPrice, BaseConditionalAccess.INCORRECT_PRICE);
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
                    string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, domainId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on Purchase key = {0}", invalidationKey);
                    }

                    // cancel existing subscription
                    Status cancelSubscriptionStatus = cas.CancelServiceNow((int)domainId, int.Parse(subscriptionInTheSameSet.m_SubscriptionCode), eTransactionType.Subscription, true, udid, userIp);
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
            Status cancelRenewalStatus = cas.CancelSubscriptionRenewal((int)domainId, subscriptionInTheSameSet.m_sObjectCode, userId, udid, userIp);

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

            var eta = previousSubsriptionPurchaseDetails.dtEndDate.AddHours(-6);
            var data = new RenewTransactionData(groupId, userId, subscriptionSetModifyDetailsId, string.Empty, 0,
                                                                 eta, eSubscriptionRenewRequestType.Downgrade);
            // enqueue scheduled purchase transaction
            var queue = new GenericCeleryQueue();
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

                long.TryParse(downgradeDetails.UserId, out long userID);
                var contextData = new ContextData(downgradeDetails.GroupId)
                {
                    UserId = userID,
                    DomainId = downgradeDetails.DomainId,
                    UserIp = downgradeDetails.UserIp,
                    Udid = downgradeDetails.UDID
                };

                TransactionResponse purchaseResponse = Purchase(cas, contextData, downgradeDetails.Price, downgradeDetails.CurrencyCode, 0,
                                                                (int)downgradeDetails.SubscriptionId, eTransactionType.Subscription, downgradeDetails.CouponCode,
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
                    bool enqueueSuccessful = true;

                    
                    var data = new RenewTransactionData(groupId, downgradeDetails.UserId, subscriptionSetModifyDetailsId, string.Empty, 0,
                            nextAttempt, eSubscriptionRenewRequestType.Downgrade);
                    var queue = new GenericCeleryQueue();
                    enqueueSuccessful = queue.Enqueue(data,
                        string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));

                    if (!enqueueSuccessful)
                    {
                        log.ErrorFormat("Failed enqueue of subscription set downgrade scheduled purchase retry, {0}", data);
                    }
                    else
                    {
                        log.DebugFormat("scheduled purchase retry successfully queued. data: {0}", data);
                    }
                }

                shouldResetModifyStatus = true;
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

                var duration = new Duration(usageModuleLifeCycle);
                DateTime oldStartDate = Utils.GetEndDateTime(duration, oldSubscriptionPurchaseDetails.dtEndDate, false);
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
        public static TransactionResponse Purchase(BaseConditionalAccess cas, ContextData contextData, double price,
            string currency, int contentId, int productId, eTransactionType transactionType, string coupon,
            int paymentGwId, int paymentMethodId, string adapterData, bool shouldIgnoreSubscriptionSetValidation = false)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = $"Purchase request: contextData {contextData}, price {price}, currency {currency}, contentId {contentId}, productId {productId}, productType {transactionType.ToString()}, " +
                               $"coupon {coupon}, paymentGwId {paymentGwId}, paymentMethodId {paymentMethodId}, adapterData {adapterData}";

            log.Debug(logString);

            // validate siteguid
            if (!contextData.UserId.HasValue || contextData.UserId == 0)
            {
                response.Status.Set((int)eResponseStatus.InvalidUser, "Illegal user ID");
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
                RolePermissions rolePermission = RolePermissions.PURCHASE_PPV;
                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        rolePermission = RolePermissions.PURCHASE_PPV;
                        break;
                    case eTransactionType.Subscription:
                        rolePermission = RolePermissions.PURCHASE_SUBSCRIPTION;
                        break;
                    case eTransactionType.Collection:
                        rolePermission = RolePermissions.PURCHASE_COLLECTION;
                        break;                   
                    default:
                        break;
                }

                if (!RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(contextData.GroupId, contextData.UserId.Value)
                    && !RolesPermissionsManager.Instance.IsPermittedPermission(contextData.GroupId, contextData.UserId.ToString(), rolePermission))
                {
                    response.Status = RolesPermissionsManager.GetSuspentionStatus(contextData.GroupId, (int)contextData.DomainId.Value);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate user           
                ResponseStatus userValidStatus = ResponseStatus.OK;
                Core.Users.User user;
                var householdId = contextData.DomainId.Value;
                userValidStatus = Utils.ValidateUser(contextData.GroupId, contextData.UserId.ToString(), ref householdId, out user, true);
                contextData.DomainId = householdId;
                if (userValidStatus != ResponseStatus.OK || user == null || user.m_oBasicData == null)
                {
                    // user validation failed
                    response.Status = Utils.SetResponseStatus(userValidStatus);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate household
                if (contextData.DomainId < 1)
                {
                    response.Status.Message = "Illegal household";
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                CouponData couponData = null;

                // coupon validation
                if (!string.IsNullOrEmpty(coupon))
                {
                    couponData = Utils.GetCouponData(contextData.GroupId, coupon, contextData.DomainId.Value);

                    if (couponData == null)
                    {
                        response.Status.Message = "Coupon Not Valid";
                        response.Status.Code = (int)eResponseStatus.CouponNotValid;
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        return response;
                    }
                }

                // get payment gateway
                PaymentGatewayItemResponse paymentGatewayResponse = null;
                if (transactionType == eTransactionType.PPV || transactionType == eTransactionType.Collection || transactionType == eTransactionType.ProgramAssetGroupOffer)
                {
                    paymentGatewayResponse = Core.Billing.Module.GetPaymentGateway(contextData.GroupId, contextData.DomainId.Value, paymentGwId, contextData.UserIp);
                }

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        response = PurchasePPV(cas, contextData, price, currency, contentId, productId, couponData, paymentGwId, paymentMethodId, adapterData, paymentGatewayResponse.PaymentGateway);
                        break;
                    case eTransactionType.Subscription:
                        response = PurchaseSubscription(cas, contextData, price, currency, productId, couponData, paymentGwId, paymentMethodId, adapterData, shouldIgnoreSubscriptionSetValidation, user);
                        break;
                    case eTransactionType.Collection:
                        response = PurchaseCollection(cas, contextData, price, currency, productId, coupon, paymentGwId, paymentMethodId, adapterData, paymentGatewayResponse.PaymentGateway);
                        break;
                    case eTransactionType.ProgramAssetGroupOffer:
                        response = PurchasePago(cas, contextData, price, currency, productId, coupon, paymentGwId, paymentMethodId, adapterData, paymentGatewayResponse.PaymentGateway, user);
                        break;
                    default:
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal product ID");
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        break;
                }

                if (response != null && response.Status != null && response.Status.Code == (int)eResponseStatus.OK)
                {
                    //update coupon usage
                    if (couponData?.id != null)
                    {
                        CouponWalletHandler.UpdateLastUsageDate(contextData.DomainId.Value, couponData.id);
                    }

                    string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(contextData.GroupId, contextData.DomainId.Value);
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


        private static TransactionResponse PurchaseCollection(BaseConditionalAccess cas, ContextData contextData, double price, string currency, int productId, string coupon,
            int paymentGwId, int paymentMethodId, string adapterData, PaymentGateway paymentGateway)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = $"Purchase request: contextData {contextData}, price {price}, currency {currency}, productId {productId }, coupon {coupon }, " +
                               $"paymentGwId {paymentGwId }, paymentMethodId {paymentMethodId } adapterData {adapterData }";

            try
            {
                // get country by user IP

                string country = string.Empty;
                if (!string.IsNullOrEmpty(contextData.UserIp))
                {
                    country = Utils.GetIP2CountryName(contextData.GroupId, contextData.UserIp);
                }

                // validate price
                Collection collection = null;
                var siteguid = contextData.UserId.ToString();
                var fullPrice = Utils.GetCollectionFinalPrice(contextData.GroupId, productId.ToString(), siteguid, coupon,
                                                              ref collection, country, string.Empty, contextData.Udid, string.Empty, contextData.UserIp, currency);

                if (fullPrice.PriceReason == PriceReason.ForPurchase)
                {
                    // item is for purchase
                    if ((fullPrice.FinalPrice != null && fullPrice.FinalPrice.m_dPrice == price && fullPrice.FinalPrice.m_oCurrency.m_sCurrencyCD3 == currency) ||
                        (paymentGateway != null && paymentGateway.ExternalVerification))
                    {
                        // price validated, create the Custom Data
                        string customData = cas.GetCustomDataForCollection(collection, productId.ToString(), siteguid, price, currency, coupon,
                                                                       contextData.UserIp, country, string.Empty, contextData.Udid, string.Empty);

                        // create new GUID for billing_transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(cas, contextData.GroupId, siteguid, contextData.DomainId.Value, price, currency, contextData.UserIp, customData, productId,
                            eTransactionType.Collection, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);

                        if (response != null && response.Status != null)
                        {
                            // Status OK + (State OK || State Pending) = grant entitlement
                            if (response.Status.Code == (int)eResponseStatus.OK &&
                               (response.State.Equals(eTransactionState.OK.ToString()) ||
                                response.State.Equals(eTransactionState.Pending.ToString())))
                            {
                                if (paymentGateway == null)
                                {
                                    paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(contextData.GroupId, contextData.DomainId.Value, billingGuid);
                                }

                                // purchase passed, update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                response.CreatedAt = DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlementDate);
                                DateTime? endDate = null;

                                bool handleBillingPassed = false;
                                long purchaseID = 0;

                                if (response.State.Equals(eTransactionState.Pending.ToString()) & paymentGateway != null && paymentGateway.IsAsyncPolicy)
                                {
                                    int pendingInMinutes = paymentGateway.GetAsyncPendingMinutes();
                                    endDate = entitlementDate.AddMinutes(pendingInMinutes);

                                    handleBillingPassed = cas.HandleCollectionBillingSuccess(ref response, siteguid, contextData.DomainId.Value, collection,
                                        price, currency, coupon, contextData.UserIp, country, contextData.Udid, long.Parse(response.TransactionID), customData, productId,
                                        billingGuid, false, entitlementDate, ref purchaseID, endDate, true);
                                    
                                    // Pending failed
                                    if (!handleBillingPassed)
                                    {
                                        response.Status = new Status((int)eResponseStatus.Error, "Pending entitlement failed");
                                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                    }

                                    return response;
                                }


                                // grant entitlement
                                handleBillingPassed =
                                    cas.HandleCollectionBillingSuccess(ref response, siteguid, contextData.DomainId.Value, collection, price, currency, coupon, contextData.UserIp,
                                        country, contextData.Udid, long.Parse(response.TransactionID), customData, productId,
                                        billingGuid, false, entitlementDate, ref purchaseID);

                                if (handleBillingPassed)
                                {
                                    cas.WriteToUserLog(siteguid, string.Format("Collection Purchase, ProductID:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                        productId, purchaseID, response.TransactionID));

                                    // entitlement passed - build notification message
                                    var dicData = new Dictionary<string, object>(){
                                        {"CollectionCode", productId}, 
                                        {"BillingTransactionID", response.TransactionID},
                                        {"SiteGUID", siteguid},
                                        {"PurchaseID", purchaseID},
                                        {"CouponCode", coupon},
                                        {"CustomData", customData}
                                    };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData, siteguid, contextData.Udid, contextData.UserIp))
                                    {
                                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }

                                    if (fullPrice.CampaignDetails != null)
                                    {
                                        Task.Run(() => HandelCampaignMessageDetailsAfterPurchase(contextData, fullPrice.CampaignDetails, eTransactionType.Collection, productId));
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
                            response.Status = new Status((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new Status((int)eResponseStatus.IncorrectPrice, BaseConditionalAccess.INCORRECT_PRICE);
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    response.Status = Utils.SetResponseStatus(fullPrice.PriceReason);
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

        private static TransactionResponse PurchaseSubscription(BaseConditionalAccess cas, ContextData contextData, double price, string currency, int productId, CouponData coupon,
                                                                int paymentGwId, int paymentMethodId, string adapterData, bool isSubscriptionSetModifySubscription, Users.User user)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;
            }

            // log request
            var logString = $"Purchase request: contextData {contextData}, price {price}, currency {currency}, productId {productId}, coupon {coupon}, paymentGwId {paymentGwId}, paymentMethodId {paymentMethodId}, adapterData {adapterData}";
            var householdId = contextData.DomainId.Value;
            var userId = contextData.UserId.ToString();

            try
            {
                string country = string.Empty;

                if (!string.IsNullOrEmpty(contextData.UserIp))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(contextData.GroupId, contextData.UserIp);
                }

                // validate price
                Subscription subscription = null;
                bool isGiftCard = false;

                var fullPrice = Utils.GetSubscriptionFullPrice(contextData.GroupId, productId.ToString(), userId, couponCode, ref subscription, country, string.Empty,
                                                                contextData.Udid, contextData.UserIp, currency, isSubscriptionSetModifySubscription);

                var subscriptionCycle = fullPrice.SubscriptionCycle;
                double couponRemainder = fullPrice.CouponRemainder;
                couponCode = fullPrice.CouponCode;
                Price finalPrice = fullPrice.FinalPrice;
                PriceReason priceReason = fullPrice.PriceReason;

                if (subscription == null)
                {
                    response.Status.Message = "ProductId doesn't exist";
                    return response;
                }

                if (fullPrice != null && ((fullPrice.PriceReason == PriceReason.PendingEntitlement)  || (fullPrice.PriceReason == PriceReason.SubscriptionPurchased && fullPrice.FinalPrice.m_dPrice == -1)))
                {
                    // item not for purchase
                    response.Status = Utils.SetResponseStatus(fullPrice.PriceReason);
                    return response;
                }

                if (string.IsNullOrEmpty(couponCode))
                {
                    coupon = null;
                }

                // check permission for subscription with premium services - by roles 
                if (subscription.m_lServices != null && subscription.m_lServices.Count() > 0)
                {
                    if (!RolesPermissionsManager.Instance.IsPermittedPermission(contextData.GroupId, userId, RolePermissions.PURCHASE_SERVICE))
                    {
                        response.Status = APILogic.Api.Managers.RolesPermissionsManager.GetSuspentionStatus(contextData.GroupId, (int)householdId);
                        log.ErrorFormat("User validation failed: {0}, data: {1}", response.Status.Message, logString);
                        return response;
                    }
                }

                var status = api.HandleBlockingSegment<SegmentBlockPurchaseSubscriptionAction>(contextData.GroupId, userId, contextData.Udid, contextData.UserIp, (int)householdId, ObjectVirtualAssetInfoType.Subscription, productId.ToString());
                if (!status.IsOkStatusCode())
                {
                    response.Status = status;
                    return response;
                }

                // if unified billing cycle is in the "history" ignore it in purchase! 
                if (subscriptionCycle.UnifiedBillingCycle != null && subscriptionCycle.UnifiedBillingCycle.endDate < DateUtils.DateTimeToUtcUnixTimestampMilliseconds(DateTime.UtcNow))
                {
                    subscriptionCycle.UnifiedBillingCycle = null;
                }

                if (paymentGwId == 0 && subscriptionCycle.PaymentGatewayId > 0)
                {
                    paymentGwId = subscriptionCycle.PaymentGatewayId;
                }
                else if (subscriptionCycle.PaymentGatewayId == 0)
                {
                    subscriptionCycle.PaymentGatewayId = paymentGwId;
                }

                var paymentGateway = Billing.Module.GetPaymentGateway(contextData.GroupId, contextData.DomainId.Value, paymentGwId, contextData.UserIp).PaymentGateway;
                if (paymentGateway != null)
                {
                    paymentGwId = paymentGateway.ID;
                }

                // if the payment gateway is external ignore unified billing !!
                if (paymentGateway != null && paymentGateway.ExternalVerification)
                {
                    log.DebugFormat("paymentGateway.ExternalVerification {0}", paymentGateway.ID);
                    subscriptionCycle.UnifiedBillingCycle = null;
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
                        ApiObjects.Response.Status Status = Utils.CanPurchaseAddOn(contextData.GroupId, householdId, subscription);
                        if (Status.Code != (int)eResponseStatus.OK)
                        {
                            response.Status = Status;
                            return response;
                        }
                    }
                    else
                    {
                        List<SubscriptionSet> groupSwitchSubscriptionSets = Pricing.Utils.GetSubscriptionSets(contextData.GroupId, null, SubscriptionSetType.Switch);
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
                                    if (IsEntitlementContainSameSetSubscription(contextData.GroupId, householdId, productId, setAndPriority.Key, ref subscriptionInTheSameSet, ref oldSubscriptionPurchaseDetails))
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
                     (subscription.GetValidSubscriptionCouponGroup(coupon.m_oCouponGroup.m_sGroupCode)?.Count > 0)))
                {
                    isGiftCard = true;
                    finalPrice = new Price()
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
                bool IsDoublePurchase = false;

                // in case Subscription purchased, and subscription is non Recurring
                // check "Block double purchase" account configuration 
                if (priceReason == PriceReason.SubscriptionPurchased && !subscription.m_bIsRecurring)
                {
                    object dbBlockDoublePurchase = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "BLOCK_DOUBLE_PURCHASE", "GROUP_ID", "=", contextData.GroupId, 60 * 60 * 24, "billing_connection");
                    bool blockDoublePurchase = false;
                    if (dbBlockDoublePurchase != null && dbBlockDoublePurchase != DBNull.Value)
                    {
                        blockDoublePurchase = ODBCWrapper.Utils.GetIntSafeVal(dbBlockDoublePurchase) == 1;
                    }

                    IsDoublePurchase = !blockDoublePurchase;
                }

                if ((priceReason == PriceReason.ForPurchase ||
                    entitleToPreview ||
                    couponFullDiscount ||
                    IsDoublePurchase) ||
                    (isGiftCard && (priceReason == PriceReason.ForPurchase || priceReason == PriceReason.Free)))
                {
                    // item is for purchase
                    if (isSubscriptionSetModifySubscription ||
                        (finalPrice != null && finalPrice.m_dPrice == price && finalPrice.m_oCurrency.m_sCurrencyCD3 == currency) ||
                        (paymentGateway != null && paymentGateway.ExternalVerification))
                    {
                        // price is validated, create custom data
                        bool partialPrice = subscriptionCycle.UnifiedBillingCycle != null && subscriptionCycle.UnifiedBillingCycle.endDate > 0 && subscriptionCycle.UnifiedBillingCycle.endDate > DateUtils.DateTimeToUtcUnixTimestampMilliseconds(DateTime.UtcNow);
                        string customData = cas.GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, userId, price, currency,
                                                                         couponCode, contextData.UserIp, country, string.Empty, contextData.Udid, string.Empty,
                                                                         entitleToPreview ? subscription.m_oPreviewModule.m_nID + "" : string.Empty,
                                                                         entitleToPreview, false, 0, false, null, partialPrice, fullPrice.CampaignDetails?.Id > 0 ? fullPrice.CampaignDetails.Id : 0);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        if (couponFullDiscount || isGiftCard)
                        {
                            response = HandleFullCouponPurchase(cas, contextData.GroupId, userId, price, currency, contextData.UserIp,
                                customData, productId, eTransactionType.Subscription, billingGuid, 0, isGiftCard);
                        }
                        else
                        {
                            response = HandlePurchase(cas, contextData.GroupId, userId, householdId, price, currency, contextData.UserIp, customData, productId,
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
                                    paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(contextData.GroupId, householdId, billingGuid);
                                }

                                // update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                DateTime? endDate = null;
                                response.CreatedAt = DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlementDate);

                                bool handleBillingPassed = false;

                                if (response.State.Equals(eTransactionState.Pending.ToString()) & paymentGateway != null && paymentGateway.IsAsyncPolicy)
                                {
                                    int pendingInMinutes = paymentGateway.GetAsyncPendingMinutes();                                   
                                    endDate = entitlementDate.AddMinutes(pendingInMinutes);

                                    handleBillingPassed =
                                    cas.HandleSubscriptionBillingSuccess(ref response, userId, householdId, subscription, price, currency, couponCode,
                                        contextData.UserIp, country, contextData.Udid, long.Parse(response.TransactionID), customData, productId, billingGuid.ToString(),
                                        entitleToPreview, subscription.m_bIsRecurring && !subscription.PreSaleDate.HasValue, entitlementDate, ref purchaseID, ref endDate, SubscriptionPurchaseStatus.OK, 0, true);

                                    // Pending failed
                                    if (!handleBillingPassed)
                                    {
                                        response.Status = new Status((int)eResponseStatus.Error, "Pending entitlement failed");
                                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                    }

                                    if (subscription.m_bIsRecurring)
                                    {
                                        var recurringRenewDetails = new RecurringRenewDetails()
                                        {
                                            CouponCode = couponCode,
                                            CouponRemainder = couponRemainder,
                                            IsCouponGiftCard = isGiftCard,
                                            IsPurchasedWithPreviewModule = entitleToPreview,
                                            LeftCouponRecurring = coupon != null ? coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon : 0,
                                            TotalNumOfRenews = 0,
                                            IsCouponHasEndlessRecurring = coupon != null && coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon == 0
                                        };

                                        if (!ConditionalAccessDAL.SaveRecurringRenewDetails(recurringRenewDetails, purchaseID))
                                        {
                                            log.ErrorFormat("Error to Insert RecurringRenewDetails to CB, purchaseId:{0}.", purchaseID);
                                        }
                                    }

                                    return response;
                                }


                                if (isGiftCard)
                                {
                                    endDate = CalculateGiftCardEndDate(cas, coupon, subscription, entitlementDate);
                                }

                                if (subscriptionCycle.UnifiedBillingCycle != null && !entitleToPreview)
                                {
                                    // calculate end date by unified billing cycle
                                    endDate = DateUtils.UtcUnixTimestampMillisecondsToDateTime(subscriptionCycle.UnifiedBillingCycle.endDate);
                                }

                                //try get from db process_purchases_id - if not exists - create one - only if pare of billing cycle
                                long processId = 0;
                                bool isNew = false;
                                if (paymentGateway != null)
                                {
                                    if (!endDate.HasValue)
                                    {
                                        endDate = Utils.CalcSubscriptionEndDate(subscription, entitleToPreview, entitlementDate);
                                    }

                                    //create process id only for subscription that equal the cycle and are NOT preview module or purchase with coupon
                                    if (!paymentGateway.ExternalVerification &&
                                        subscription != null &&
                                        !subscription.PreSaleDate.HasValue &&
                                        subscription.m_bIsRecurring &&
                                        subscription.m_MultiSubscriptionUsageModule != null &&
                                        subscription.m_MultiSubscriptionUsageModule.Count() == 1 /*only one price plan*/ &&
                                        subscriptionCycle.HasCycle)
                                    // group define with billing cycle
                                    {
                                        if (subscriptionCycle.UnifiedBillingCycle == null || !entitleToPreview)
                                        {
                                            processId = Utils.GetUnifiedProcessId(contextData.GroupId, paymentGateway.ID, endDate.Value, householdId, subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle, out isNew);
                                        }
                                    }
                                }

                                // in case of - non-recurring  start date should be the entitlements end date
                                //-------------------------------------------------------
                                if (IsDoublePurchase)
                                {
                                    // Get all user entitlements
                                    DomainEntitlements domainEntitlements = null;
                                    if (!Utils.TryGetDomainEntitlementsFromCache(contextData.GroupId, (int)householdId, null, ref domainEntitlements))
                                    {
                                        log.Error($"Utils.GetUserEntitlements, groupId: {contextData.GroupId}, domainId: {householdId}");
                                    }

                                    var userBundlePurchase = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.FirstOrDefault(x => x.Key == productId.ToString()).Value;
                                    if (userBundlePurchase != null)
                                    {
                                        response.StartDateSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(userBundlePurchase.dtEndDate);
                                        endDate = Utils.CalcSubscriptionEndDate(subscription, entitleToPreview, userBundlePurchase.dtEndDate);
                                        response.EndDateSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate.Value);
                                    }
                                }

                                // grant entitlement
                                handleBillingPassed =
                                    cas.HandleSubscriptionBillingSuccess(ref response, userId, householdId, subscription, price, currency, couponCode,
                                        contextData.UserIp, country, contextData.Udid, long.Parse(response.TransactionID), customData, productId, billingGuid.ToString(),
                                        entitleToPreview, subscription.m_bIsRecurring && !subscription.PreSaleDate.HasValue, entitlementDate, ref purchaseID, ref endDate, SubscriptionPurchaseStatus.OK, processId);

                                if (handleBillingPassed && endDate.HasValue)
                                {
                                    cas.WriteToUserLog(userId, string.Format("Subscription Purchase, productId:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                        productId, purchaseID, response.TransactionID));

                                    // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                                    if (subscription.m_nDomainLimitationModule != 0 && !IsDoublePurchase)
                                    {
                                        cas.UpdateDLM(householdId, subscription.m_nDomainLimitationModule);
                                    }

                                    if (fullPrice.CampaignDetails != null)
                                    {
                                        Task.Run(() => HandelCampaignMessageDetailsAfterPurchase(contextData, fullPrice.CampaignDetails, eTransactionType.Subscription, productId));
                                    }
                                    
                                    long endDateUnix = endDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate.Value) : 0;

                                    // If the subscription if recurring, put a message for renewal and all that...
                                    if (subscription.m_bIsRecurring && !subscription.PreSaleDate.HasValue)
                                    {
                                        var recurringRenewDetails = new RecurringRenewDetails()
                                        {
                                            CouponCode = couponCode,
                                            CouponRemainder = couponRemainder,
                                            IsCouponGiftCard = isGiftCard,
                                            IsPurchasedWithPreviewModule = entitleToPreview,
                                            LeftCouponRecurring = coupon != null ? coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon : 0,
                                            TotalNumOfRenews = 0,
                                            IsCouponHasEndlessRecurring = coupon != null && coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon == 0,
                                            CampaignDetails = fullPrice.CampaignDetails
                                        };

                                        if (!ConditionalAccessDAL.SaveRecurringRenewDetails(recurringRenewDetails, purchaseID))
                                        {
                                            log.ErrorFormat("Error to Insert RecurringRenewDetails to CB, purchaseId:{0}.", purchaseID);
                                        }

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

                                                    if (!paymentGateway.ExternalVerification && subscriptionCycle.UnifiedBillingCycle == null &&
                                                        subscription.m_bIsRecurring && subscription.m_MultiSubscriptionUsageModule != null &&
                                                        subscription.m_MultiSubscriptionUsageModule.Count() == 1)
                                                    {
                                                        Utils.HandleDomainUnifiedBillingCycle(contextData.GroupId, householdId, ref subscriptionCycle, subscription.m_MultiSubscriptionUsageModule[0].m_tsMaxUsageModuleLifeCycle, endDate.Value, !string.IsNullOrEmpty(couponCode));
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
                                            PurchaseManager.SendGiftCardReminderEmails(cas, contextData.GroupId, endDateUnix, nextRenewalDate, userId, householdId, purchaseID, billingGuid);
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
                                            bool isKronos = PhoenixFeatureFlagInstance.Get().IsUnifiedRenewUseKronos();
                                            Utils.RenewUnifiedTransactionMessageInQueue(contextData.GroupId, householdId, DateUtils.DateTimeToUtcUnixTimestampMilliseconds(endDate.Value), nextRenewalDate, processId, isKronos);
                                        }
                                        else if (subscriptionCycle.UnifiedBillingCycle == null || ((entitleToPreview || !string.IsNullOrEmpty(couponCode)) && !isNew))
                                        {
                                            // insert regular message 
                                            RenewTransactionMessageInQueue(contextData.GroupId, userId, billingGuid, purchaseID, endDateUnix, nextRenewalDate, householdId);
                                        }

                                        //else do nothing, message already exists

                                        #endregion
                                    }
                                    else
                                    // If subscription is not recurring, enqueue subscription ends message
                                    {
                                        object dbBlockDoublePurchase = ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "BLOCK_DOUBLE_PURCHASE", "GROUP_ID", "=", contextData.GroupId, 60 * 60 * 24, "billing_connection");
                                        bool blockDoublePurchase = false;
                                        if (dbBlockDoublePurchase != null && dbBlockDoublePurchase != DBNull.Value)
                                        {
                                            blockDoublePurchase = ODBCWrapper.Utils.GetIntSafeVal(dbBlockDoublePurchase) == 1;
                                        }

                                        if (!blockDoublePurchase && endDate <= DateTime.Now.AddYears(1)) // reminder message
                                        {
                                            RenewTransactionData data = new RenewTransactionData(contextData.GroupId, userId, purchaseID, billingGuid,
                                            endDateUnix, endDate.Value, eSubscriptionRenewRequestType.Reminder);
                                            PurchaseManager.SendRenewalReminder(cas.m_nGroupID, data, householdId);
                                        }

                                        bool isKronos = PhoenixFeatureFlagInstance.Get().IsRenewSubscriptionEndsUseKronos();
                                        RenewManager.EnqueueSubscriptionEndsMessage(contextData.GroupId, userId, purchaseID, endDateUnix, isKronos);
                                    }

                                    // build notification message
                                    var dicData = new Dictionary<string, object>()
                                    {
                                        {"SubscriptionCode", productId},
                                        {"BillingTransactionID", response.TransactionID},
                                        {"SiteGUID", userId},
                                        {"PurchaseID", purchaseID},
                                        {"CouponCode", couponCode},
                                        {"CustomData", customData}
                                    };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData, userId, contextData.Udid, contextData.UserIp))
                                    {
                                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }
                                }
                                else
                                {
                                    // purchase passed, entitlement failed
                                    response.Status = new Status((int)eResponseStatus.PurchasePassedEntitlementFailed, BaseConditionalAccess.PURCHASE_PASSED_ENTITLEMENT_FAILED);
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
                            response.Status = new Status((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new Status((int)eResponseStatus.IncorrectPrice, BaseConditionalAccess.INCORRECT_PRICE);
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

        private static void HandelCampaignMessageDetailsAfterPurchase(ContextData contextData, RecurringCampaignDetails recurringCampaignDetails, eTransactionType productType, int productId)
        {
            if (recurringCampaignDetails == null || recurringCampaignDetails.Id <= 0) { return; }

            //Update campaign message details
            var domainResponse = Domains.Module.GetDomainInfo(contextData.GroupId, (int)contextData.DomainId);
            long _userId = domainResponse.Domain.m_masterGUIDs.FirstOrDefault();

            var userCampaigns = CampaignUsageRepository.Instance.GetCampaignInboxMessageMapCB(contextData.GroupId, _userId);

            if (userCampaigns.Campaigns.ContainsKey(recurringCampaignDetails.Id))
            {
                long now = DateUtils.GetUtcUnixTimestampNow();
                
                // handle anti fraud for Subscription only
                if (productType == eTransactionType.Subscription)
                {
                    var campaignDetails = userCampaigns.Campaigns[recurringCampaignDetails.Id];
                    campaignDetails.SubscriptionUses.Add(productId, now);
                    CampaignUsageRepository.Instance.SaveToCampaignInboxMessageMapCb(recurringCampaignDetails.Id, contextData.GroupId, _userId, campaignDetails);
                }
                
                if (!string.IsNullOrEmpty(recurringCampaignDetails.Udid))
                {
                    //ANTI FRAUD BEO-8610
                    CampaignUsageRepository.Instance.SaveToDeviceTriggerCampaignsUses(contextData.GroupId, recurringCampaignDetails.Udid, recurringCampaignDetails.Id, now);
                }
            }
            else
            {
                var campaignResponse = ApiLogic.Users.Managers.CampaignManager.Instance.Get(contextData, recurringCampaignDetails.Id);
                if (campaignResponse.HasObject())
                {
                    Notification.MessageInboxManger.Instance.AddCampaignMessageToUser(campaignResponse.Object, contextData.GroupId, _userId, null, productId, productType);
                }
            }

            // set campaign to HouseholdUsages
            var expiration = DateUtils.UtcUnixTimestampSecondsToDateTime(recurringCampaignDetails.CampaignEndDate);
            CampaignUsageRepository.Instance.SetCampaignHouseholdUsage(contextData.GroupId, contextData.DomainId.Value, recurringCampaignDetails.Id, expiration);
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
                    DateTime eta = endDate.AddDays(-1 * renewalReminderSettings);

                    if (eta > DateTime.UtcNow)
                    {
                        bool enqueueSuccessful = true;
                        var data = new RenewTransactionData(groupId, masterSiteGuid, purchaseId, billingGuid,
                        endDateUnix, eta, eSubscriptionRenewRequestType.RenewalReminder);
                         if (PhoenixFeatureFlagInstance.Get().IsRenewalReminderUseKronos())
                         {
                             log.Debug($"Kronos - RenewalReminder purchaseID:{purchaseId}");
                             RenewManager.addEventToKronos(groupId, data);
                         }
                         else
                        {
                            RenewTransactionsQueue queue = new RenewTransactionsQueue();
                            enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                        }
                        
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

        public static void SendRenewalReminder(int groupId, RenewTransactionData data, long householdId)
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
                        DateTime eta = data.ETA.Value.AddDays(-1 * renewalReminderSettings);

                        if (eta > DateTime.UtcNow)
                        {
                            var type = data.type == eSubscriptionRenewRequestType.Reminder ? eSubscriptionRenewRequestType.Reminder : eSubscriptionRenewRequestType.RenewalReminder;
                            var newData = new RenewTransactionData(data.GroupId, masterSiteGuid, data.purchaseId, data.billingGuid,
                            data.endDate, eta, type);

                            if (PhoenixFeatureFlagInstance.Get().IsRenewalReminderUseKronos())
                            {
                                log.Debug($"Kronos - RenewalReminder purchaseID:{data.purchaseId}");
                                RenewManager.addEventToKronos(groupId, data);
                            }
                            else
                            {
                                if (data.ETA.Value > DateTime.Now.AddYears(1))
                                {
                                    //BEO-11219
                                    log.Debug($"BEO-11219 - skip Enqueue SendRenewalReminder msg (more then 1 year)! purchaseId:{data.purchaseId}, endDateUnix:{data.endDate}");
                                    return;
                                }

                                var queue = new RenewTransactionsQueue();
                                bool enqueueSuccessful = queue.Enqueue(newData,
                                    string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, data.GroupId));

                                if (!enqueueSuccessful)
                                {
                                    log.ErrorFormat("Failed enqueue of reminder transaction {0}", data);
                                    return;
                                }
                            }

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

        public static void SendRenewalReminder(int groupId, RenewUnifiedData data)
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
                        bool enqueueSuccessful = true;
                        RenewUnifiedData newData = new RenewUnifiedData(data.GroupId, data.householdId, data.processId,
                            data.endDate, eta, eSubscriptionRenewRequestType.RenewalReminder);
                        if (PhoenixFeatureFlagInstance.Get().IsUnifiedRenewUseKronos())
                        {
                            RenewManager.addEventToKronos(groupId, newData);
                        }
                        else
                        {
                            enqueueSuccessful = queue.Enqueue(newData, string.Format(Utils.ROUTING_KEY_PROCESS_UNIFIED_RENEW_SUBSCRIPTION, data.GroupId));
                        }
                        
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

        public static bool RenewTransactionMessageInQueue(int groupId, string siteguid, string billingGuid,
            long purchaseID, long endDateUnix, DateTime nextRenewalDate, long householdId = 0)
        {
            RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseID, billingGuid, endDateUnix, nextRenewalDate);

            if (PhoenixFeatureFlagInstance.Get().IsRenewUseKronos())
            {
                ConditionalAccessDAL.Insert_SubscriptionsPurchasesKronos(purchaseID);
                
                log.Debug($"Kronos - Renewal purchaseID:{purchaseID}");
                RenewManager.addEventToKronos(groupId, data);
            }
            else
            {
                if (nextRenewalDate > DateTime.UtcNow.AddYears(1).AddDays(5))
                {
                    //BEO-11219
                    log.Debug($"BEO-11219 - skip Enqueue renew msg (more then 1 year)! purchaseID:{purchaseID}, endDateUnix:{endDateUnix}");
                    return true;
                }

                var queue = new RenewTransactionsQueue();
                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                    return false;
                }
            }

            log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}",
                nextRenewalDate, data);

            PurchaseManager.SendRenewalReminder(groupId, data, householdId);

            return true;
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
                            var data = new RenewTransactionData(groupId, masterSiteGuid, purchaseId, billingGuid,
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

        private static TransactionResponse PurchasePPV(BaseConditionalAccess cas, ContextData contextData, double price, string currency, int contentId, int productId, CouponData coupon, int paymentGwId,
                                                       int paymentMethodId, string adapterData, PaymentGateway paymentGateway)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;
            }

            // log request
            string logString = $"PPV Purchase request: contextData {contextData}, price {price}, currency {currency}, contentId {contentId}, productId {productId}, coupon {coupon}, " +
                               $"paymentGwId {paymentGwId}, paymentMethodId {paymentMethodId}, adapterData {adapterData}";

            try
            {
                // validate content ID
                if (contentId < 1)
                {
                    response.Status.Set((int)eResponseStatus.InvalidContentId, BaseConditionalAccess.ILLEGAL_CONTENT_ID);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate content ID related media
                int mediaID = Utils.GetMediaIDFromFileID(contentId, contextData.GroupId);
                if (mediaID < 1)
                {
                    response.Status.Set((int)eResponseStatus.NoMediaRelatedToFile, BaseConditionalAccess.CONTENT_ID_WITH_NO_RELATED_MEDIA);
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                var rules = AssetUserRuleManager.GetMediaAssetUserRulesToUser(contextData.GroupId, contextData.UserId.Value, mediaID);
                if (rules != null && rules.Count > 0)
                {
                    response.Status.Message = "Asset is blocked for user";
                    log.ErrorFormat("Error: {0}", response.Status.Message);
                    return response;
                }

                // validate PPV 
                PPVModule ppvModule = null;
                Status status = Utils.ValidatePPVModuleCode(contextData.GroupId, productId, contentId, ref ppvModule);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = status;
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate price
                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;
                var siteguid = contextData.UserId.ToString();
                bool isGiftCard = false;

                var fullPrice = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(contentId, ppvModule, siteguid, couponCode,
                        contextData.GroupId, ref relevantSub, ref relevantCol, ref relevantPP, string.Empty, string.Empty, contextData.Udid,
                        false, contextData.UserIp, currency, BlockEntitlementType.NO_BLOCK);

                // get country by user IP
                string country = string.Empty;
                if (!string.IsNullOrEmpty(contextData.UserIp))
                {
                    country = Utils.GetIP2CountryName(contextData.GroupId, contextData.UserIp);
                }

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

                bool couponFullDiscount = (fullPrice.PriceReason == PriceReason.Free) && coupon != null;

                if ((fullPrice.PriceReason == PriceReason.ForPurchase ||
                    (fullPrice.PriceReason == PriceReason.SubscriptionPurchased && fullPrice.FinalPrice.m_dPrice > 0) ||
                    couponFullDiscount) ||
                    (isGiftCard && (fullPrice.PriceReason == PriceReason.ForPurchase || fullPrice.PriceReason == PriceReason.Free)))
                {
                    if (isGiftCard)
                    {
                        fullPrice.PriceReason = PriceReason.Free;
                        fullPrice.FinalPrice = new Price()
                        {
                            m_dPrice = 0.0,
                            m_oCurrency = new Currency()
                            {
                                m_sCurrencyCD3 = currency
                            }
                        };
                    }

                    // item is for purchase
                    if ((fullPrice.FinalPrice.m_dPrice == price && fullPrice.FinalPrice.m_oCurrency.m_sCurrencyCD3 == currency)
                        || (paymentGateway != null && paymentGateway.ExternalVerification))
                    {
                        // create custom data
                        string customData = cas.GetCustomData
                            (relevantSub, ppvModule, null, siteguid, price, currency, contentId, mediaID, productId.ToString(), string.Empty, couponCode,
                             contextData.UserIp, country, string.Empty, contextData.Udid, contextData.DomainId.Value, fullPrice.CampaignDetails?.Id ?? 0);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        if (couponFullDiscount || isGiftCard)
                        {
                            response = HandleFullCouponPurchase(cas, contextData.GroupId, siteguid, price, currency, contextData.UserIp, customData,
                                productId, eTransactionType.PPV, billingGuid, contentId, isGiftCard);
                        }
                        else
                        {
                            response = HandlePurchase(cas, contextData.GroupId, siteguid, contextData.DomainId.Value, price, currency, contextData.UserIp, customData, productId,
                                eTransactionType.PPV, billingGuid, paymentGwId, contentId, paymentMethodId, adapterData);
                        }

                        if (response != null && response.Status != null)
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
                                response.CreatedAt = DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlementDate);

                                if (paymentGateway == null)
                                {
                                    paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(contextData.GroupId, contextData.DomainId.Value, billingGuid);
                                }

                                bool handleBillingPassed = false;
                                DateTime? endDate = null;    
                                if (response.State.Equals(eTransactionState.Pending.ToString()) & paymentGateway != null && paymentGateway.IsAsyncPolicy)
                                {                                    
                                    int pendingInMinutes = paymentGateway.GetAsyncPendingMinutes();
                                    endDate = entitlementDate.AddMinutes(pendingInMinutes);

                                    handleBillingPassed = cas.HandlePPVBillingSuccess(ref response, siteguid, contextData.DomainId.Value, relevantSub, price, 
                                        currency, couponCode, contextData.UserIp, country, contextData.Udid, long.Parse(response.TransactionID), customData, 
                                        ppvModule, productId, contentId, billingGuid, entitlementDate, ref purchaseId, endDate, true);

                                    // Pending failed
                                    if (!handleBillingPassed)
                                    {
                                        response.Status = new Status((int)eResponseStatus.Error, "Pending entitlement failed");
                                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                                    }

                                    return response;
                                }

                                // grant entitlement
                                handleBillingPassed = cas.HandlePPVBillingSuccess(ref response, siteguid, contextData.DomainId.Value, relevantSub, price, currency, couponCode, contextData.UserIp,
                                    country, contextData.Udid, long.Parse(response.TransactionID), customData, ppvModule, productId, contentId,
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
                                        {"CouponCode", couponCode},
                                        {"CustomData", customData},
                                        {"PurchaseID", purchaseId},
                                        {"ClientIp", contextData.UserIp}
                                    };

                                    // notify purchase
                                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData, siteguid, contextData.Udid, contextData.UserIp))
                                    {
                                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", response.Status.Message, logString);
                                    }

                                    if (fullPrice.CampaignDetails != null)
                                    {
                                        Task.Run(() => HandelCampaignMessageDetailsAfterPurchase(contextData, fullPrice.CampaignDetails, eTransactionType.PPV, productId));
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
                            response.Status = new Status((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                            log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                        }
                    }
                    else
                    {
                        // incorrect price
                        response.Status = new Status((int)eResponseStatus.IncorrectPrice, BaseConditionalAccess.INCORRECT_PRICE);
                        log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    }
                }
                else
                {
                    // not for purchase
                    response.Status = Utils.SetResponseStatus(fullPrice.PriceReason);
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

        private static TransactionResponse PurchasePago(BaseConditionalAccess cas, ContextData contextData, double price, string currency, int pagoId, string coupon,
            int paymentGwId, int paymentMethodId, string adapterData, PaymentGateway paymentGateway, Users.User user)
        {
            TransactionResponse response = new TransactionResponse(eResponseStatus.Error);

            // log request
            string logString = $"Purchase request: contextData {contextData}, price {price}, currency {currency}, pagoId {pagoId }, coupon {coupon }, " +
                               $"paymentGwId {paymentGwId }, paymentMethodId {paymentMethodId } adapterData {adapterData }";
            try
            {
                // get country by user IP
                string country = string.Empty;
                if (!string.IsNullOrEmpty(contextData.UserIp))
                {
                    country = Utils.GetIP2CountryName(contextData.GroupId, contextData.UserIp);
                }

                // get pago
                ProgramAssetGroupOffer pago = PagoManager.Instance.GetProgramAssetGroupOffer(contextData.GroupId, pagoId);
                if (pago == null)
                {
                    response.Status.Set(eResponseStatus.Error, "ProductId doesn't exist");
                    log.Warn($"Warning: {response.Status.Message}, data: {logString}");
                    return response;
                }

                if (!PagoManager.Instance.IsPagoAllowed(pago))
                {
                    response.Status = Utils.SetResponseStatus(PriceReason.NotForPurchase);                    
                    log.Warn($"Warning: This programAssetGroupOffer is NotForPurchase , data: {logString}");
                    return response;
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = PriceManager.GetPagoFinalPrice(contextData.GroupId, contextData.UserId.Value, ref priceReason, pago, country, contextData.UserIp,
                    currency);

                if (priceReason != PriceReason.ForPurchase)
                {
                    // not for purchase
                    response.Status = Utils.SetResponseStatus(priceReason);
                    log.Warn($"Warning: {response.Status.Message}, data: {logString}");
                    return response;
                }

                // item is for purchase
                if ((priceResponse != null && priceResponse.m_dPrice == price && priceResponse.m_oCurrency.m_sCurrencyCD3 == currency) ||
                    (paymentGateway != null && paymentGateway.ExternalVerification))
                {
                    // price validated, create the Custom Data
                    string customData = cas.GetCustomDataForPago(pago, pagoId, contextData.UserId.Value, price, currency, contextData.UserIp, country, contextData.Udid);

                    // create new GUID for billing_transaction
                    string billingGuid = Guid.NewGuid().ToString();

                    // purchase
                    response = HandlePurchase(cas, contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value, price, currency, contextData.UserIp, customData,
                        pagoId, eTransactionType.ProgramAssetGroupOffer, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);

                    if (response == null || response.Status == null)
                    {
                        // purchase failed - no status error
                        response.Status = new Status((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                        log.Warn($"Warn: {response.Status.Message}, data: {logString}");
                        return response;
                    }

                    // Status OK + (State OK || State Pending) = grant entitlement
                    if (response.Status.Code == (int)eResponseStatus.OK && (response.State.Equals(eTransactionState.OK.ToString()) ||
                        response.State.Equals(eTransactionState.Pending.ToString())))
                    {
                        if (paymentGateway == null)
                        {
                            paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(contextData.GroupId, contextData.DomainId.Value, billingGuid);
                        }

                        // purchase passed, update entitlement date
                        DateTime entitlementDate = DateTime.UtcNow;
                        response.CreatedAt = DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlementDate);
                        DateTime? endDate = null;

                        bool handleBillingPassed = false;
                        long purchaseID = 0;

                        if (response.State.Equals(eTransactionState.Pending.ToString()) & paymentGateway != null && paymentGateway.IsAsyncPolicy)
                        {
                            int pendingInMinutes = paymentGateway.GetAsyncPendingMinutes();
                            endDate = entitlementDate.AddMinutes(pendingInMinutes);

                            handleBillingPassed = cas.HandlePagoBillingSuccess(ref response, contextData.UserId.Value, contextData.DomainId.Value, pago, price, currency,
                                contextData.UserIp, country, contextData.Udid, long.Parse(response.TransactionID), customData, pagoId, billingGuid, false, entitlementDate,
                                ref purchaseID, endDate, true);

                            // Pending failed
                            if (!handleBillingPassed)
                            {
                                response.Status = new Status((int)eResponseStatus.Error, "Pending entitlement failed");
                                log.Warn($"Warn: {response.Status.Message}, data: {logString}");
                            }

                            return response;
                        }

                        // grant entitlement
                        cas.HandlePagoBillingSuccess(ref response, contextData.UserId.Value, contextData.DomainId.Value, pago, price, currency, contextData.UserIp,
                                country, contextData.Udid, long.Parse(response.TransactionID), customData, pagoId, billingGuid, false, entitlementDate, ref purchaseID);
                    }
                    else
                    {
                        // purchase failed - received error status
                        log.Warn($"Warn: {response.Status.Message}, data: {logString}");
                    }

                }
                else
                {
                    // incorrect price
                    response.Status = new Status((int)eResponseStatus.IncorrectPrice, BaseConditionalAccess.INCORRECT_PRICE);
                    log.Warn($"Warn: {response.Status.Message}, data: {log}");
                }

            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
            }
            return response;
        }

        #endregion
    }
}
