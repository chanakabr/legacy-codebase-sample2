using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using Billing;
using DAL;
using KLogMonitor;
using Pricing;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using WS_Billing;

namespace ConditionalAccess
{
    public class PurchaseManager
    {
        #region Consts
        
        private const string ILLEGAL_CONTENT_ID = "Illegal content ID";
        protected const string ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION = "PROCESS_RENEW_SUBSCRIPTION\\{0}";

        #endregion        

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Static Methods

        /// <summary>
        /// Purchase
        /// </summary>
        public static TransactionResponse Purchase(BaseConditionalAccess cas, int groupId, string siteguid, long household, double price, string currency, int contentId, int productId,
                                                 eTransactionType transactionType, string coupon, string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData)
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
                // validate user
                ResponseStatus userValidStatus = ResponseStatus.OK;
                userValidStatus = Utils.ValidateUser(groupId, siteguid, ref household);

                if (userValidStatus != ResponseStatus.OK)
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

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                    response = PurchasePPV(cas, groupId, siteguid, household, price, currency, contentId, productId, couponData, userIp, deviceName, paymentGwId, paymentMethodId, adapterData);
                    break;
                    case eTransactionType.Subscription:
                    response = PurchaseSubscription(cas, groupId, siteguid, household, price, currency, productId, couponData, userIp, deviceName, paymentGwId, paymentMethodId, adapterData);
                    break;
                    case eTransactionType.Collection:
                    response = PurchaseCollection(cas, groupId, siteguid, household, price, currency, productId, coupon, userIp, deviceName, paymentGwId, paymentMethodId, adapterData);
                    break;
                    default:
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal product ID");
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    break;
                }

                if (response != null && response.Status != null && response.Status.Code == (int)eResponseStatus.OK)
                {
                    string type = string.Empty;

                    switch (transactionType)
                    {
                        case eTransactionType.PPV:
                        {
                            type = "ppv_purchase";
                            break;
                        }
                        case eTransactionType.Subscription:
                        {
                            type = "subscription_purchase";
                            break;
                        }
                        case eTransactionType.Collection:
                        {
                            type = "collection_purchase";
                            break;
                        }
                        default:
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Purchase Error. data: {0}", logString, ex));
            }

            return response;
        }

        private static TransactionResponse PurchaseCollection(BaseConditionalAccess cas, int groupId, string siteguid, long householdId, double price, string currency, int productId, string coupon, string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, userIp {6}, deviceName {7}, paymentGwId {8}, paymentMethodId {9} adapterData {10}",
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
                    country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = null;
                Collection collection = null;
                priceResponse = Utils.GetCollectionFinalPrice(groupId, productId.ToString(), siteguid, coupon, ref priceReason,
                                                              ref collection, country, string.Empty, deviceName, string.Empty);

                bool isEntitledToPreviewModule = priceReason == PriceReason.EntitledToPreviewModule;

                if (priceReason == PriceReason.ForPurchase ||
                    isEntitledToPreviewModule)
                {
                    // item is for purchase
                    if (priceResponse != null &&
                        priceResponse.m_dPrice == price &&
                        priceResponse.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        // price validated, create the Custom Data
                        string customData = cas.GetCustomDataForCollection(collection, productId.ToString(), siteguid, price, currency, coupon,
                                                                       userIp, country, string.Empty, deviceName, string.Empty);

                        // create new GUID for billing_transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        response = HandlePurchase(cas, groupId, siteguid, householdId, price, currency, userIp, customData, productId, eTransactionType.Collection, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);
                        if (response != null &&
                            response.Status != null)
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
                                bool handleBillingPassed = cas.HandleCollectionBillingSuccess(ref response, siteguid, householdId, collection, price, currency, coupon, userIp,
                                                                                          country, deviceName, long.Parse(response.TransactionID), customData, productId,
                                                                                          billingGuid, isEntitledToPreviewModule, entitlementDate, ref purchaseID);

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

        private static TransactionResponse PurchaseSubscription(BaseConditionalAccess cas, int groupId, string siteguid, long householdId, double price, string currency, int productId,
                                                      CouponData coupon, string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;
            }

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, productId {4}, coupon {5}, userIp {6}, deviceName {7}, paymentGwId {8}, paymentMethodId {9}, adapterData {10}",
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
                    country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription subscription = null;

                bool isGiftCard = false;
                Price priceResponse = null;

                priceResponse =
                        Utils.GetSubscriptionFinalPrice(groupId, productId.ToString(), siteguid, couponCode, ref priceReason, ref subscription, country, string.Empty, deviceName);

                if (coupon != null &&
                    coupon.m_CouponStatus == CouponsStatus.Valid &&
                    coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard &&
                    subscription != null &&
                    subscription.m_oCouponsGroup != null &&
                    coupon.m_oCouponGroup != null &&
                    subscription.m_oCouponsGroup.m_sGroupCode == coupon.m_oCouponGroup.m_sGroupCode)
                {
                    isGiftCard = true;
                }

                bool entitleToPreview = priceReason == PriceReason.EntitledToPreviewModule;
                bool couponFullDiscount = (priceReason == PriceReason.Free && coupon != null);

                if (priceReason == PriceReason.ForPurchase ||
                    entitleToPreview ||
                    couponFullDiscount ||
                    isGiftCard)
                {
                    // item is for purchase
                    if (priceResponse != null &&
                        priceResponse.m_dPrice == price &&
                        priceResponse.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        // price is validated, create custom data
                        string customData = cas.GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, siteguid, price, currency,
                                                                         couponCode, userIp, country, string.Empty, deviceName, string.Empty,
                                                                         entitleToPreview ? subscription.m_oPreviewModule.m_nID + "" : string.Empty,
                                                                         entitleToPreview);

                        // create new GUID for billing transaction
                        string billingGuid = Guid.NewGuid().ToString();

                        // purchase
                        if (couponFullDiscount || isGiftCard)
                        {
                            response = HandleFullCouponPurchase(cas, groupId, siteguid, price, currency, userIp, customData, productId, eTransactionType.Subscription, billingGuid, 0, isGiftCard);
                        }
                        else
                        {
                            response = HandlePurchase(cas, groupId, siteguid, householdId, price, currency, userIp, customData, productId,
                                                      eTransactionType.Subscription, billingGuid, paymentGwId, 0, paymentMethodId, adapterData);
                        }
                        if (response != null &&
                            response.Status != null)
                        {
                            // Status OK + (State OK || State Pending) = grant entitlement
                            if (response.Status.Code == (int)eResponseStatus.OK &&
                               (response.State.Equals(eTransactionState.OK.ToString()) ||
                                response.State.Equals(eTransactionState.Pending.ToString())))
                            {
                                // purchase passed
                                long purchaseID = 0;

                                // update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                DateTime? endDate = null;
                                response.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                                if (isGiftCard)
                                {
                                    endDate = CalculateGiftCardEndDate(cas, coupon, subscription, entitlementDate);
                                }

                                // grant entitlement
                                bool handleBillingPassed = cas.HandleSubscriptionBillingSuccess(ref response, siteguid, householdId, subscription, price, currency, couponCode, userIp,
                                                                                      country, deviceName, long.Parse(response.TransactionID), customData, productId,
                                                                                      billingGuid.ToString(), entitleToPreview, subscription.m_bIsRecurring, entitlementDate, 
                                                                                      ref purchaseID, ref endDate, SubscriptionPurchaseStatus.OK);

                                if (handleBillingPassed && endDate.HasValue)
                                {
                                    cas.WriteToUserLog(siteguid, string.Format("Subscription Purchase, productId:{0}, PurchaseID:{1}, BillingTransactionID:{2}",
                                        productId, purchaseID, response.TransactionID));

                                    // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                                    if (subscription.m_nDomainLimitationModule != 0)
                                    {
                                        cas.UpdateDLM(householdId, subscription.m_nDomainLimitationModule);
                                    }

                                    if (subscription.m_bIsRecurring)
                                    {
                                        PaymentGateway paymentGatewayResponse = null;
                                        try
                                        {
                                            // call billing process renewal
                                            string billingUserName = string.Empty;
                                            string billingPassword = string.Empty;
                                            module wsBillingService = null;
                                            cas.InitializeBillingModule(ref wsBillingService, ref billingUserName, ref billingPassword);
                                            paymentGatewayResponse = wsBillingService.GetPaymentGatewayByBillingGuid(billingUserName, billingPassword, householdId, billingGuid);

                                            DateTime nextRenewalDate = endDate.Value.AddMinutes(-5); // default                                           

                                            if (paymentGatewayResponse == null)
                                            {
                                                // error getting PG
                                                log.Error("Error getting the PG - GetPaymentGatewayByBillingGuid");
                                            }
                                            else
                                            {
                                                nextRenewalDate = endDate.Value.AddMinutes(paymentGatewayResponse.RenewalStartMinutes);
                                            }

                                            // enqueue renew transaction
                                            RenewTransactionsQueue queue = new RenewTransactionsQueue();

                                            RenewTransactionData data = new RenewTransactionData(groupId, siteguid, purchaseID, billingGuid,
                                                TVinciShared.DateUtils.DateTimeToUnixTimestamp((DateTime)endDate), nextRenewalDate);
                                            bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                                            if (!enqueueSuccessful)
                                            {
                                                log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                                            }
                                            else
                                                log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}", nextRenewalDate, data);
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error("Error while trying to get the PG", ex);
                                        }
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

        private static DateTime? CalculateGiftCardEndDate(BaseConditionalAccess cas, CouponData coupon, Subscription subscription, DateTime entitlementDate)
        {
            // Calculate first end date with normal rules
            var initialEndDate = cas.CalcSubscriptionEndDate(subscription, false, entitlementDate);

            // get the time span between now and the first period end date
            var timeSpan = initialEndDate - entitlementDate;

            // expand (multiply) this period by the coupon's recurring cound
            var newTimeSpan = new TimeSpan(timeSpan.Ticks * coupon.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon);

            // new end date is now + X times the initial period
            DateTime endDate = initialEndDate + newTimeSpan;

            return endDate;
        }


        private static TransactionResponse PurchasePPV(BaseConditionalAccess cas, int groupId, 
            string siteguid, long householdId, double price, 
            string currency, int contentId, int productId, CouponData coupon, string userIp, string deviceName, int paymentGwId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            string couponCode = string.Empty;

            if (coupon != null)
            {
                couponCode = coupon.id;
            }

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, price {2}, currency {3}, contentId {4}, productId {5}, coupon {6}, userIp {7}, deviceName {8}, paymentGwId {9}, paymentMethodId {10}, adapterData {11}",
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
                ApiObjects.Response.Status status = cas.ValidatePPVModuleCode(productId, contentId, ref ppvModule);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    response.Status = status;
                    log.ErrorFormat("Error: {0}, data: {1}", response.Status.Message, logString);
                    return response;
                }

                // validate price
                PriceReason ePriceReason = PriceReason.UnKnown;
                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;

                bool isGiftCard = false;
                Price priceObject = null;

                if (coupon != null &&
                    coupon.m_CouponStatus == CouponsStatus.Valid &&
                    coupon.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard &&
                    ppvModule != null &&
                    ppvModule.m_oCouponsGroup != null &&
                    coupon.m_oCouponGroup != null &&
                    ppvModule.m_oCouponsGroup.m_sGroupCode == coupon.m_oCouponGroup.m_sGroupCode)
                {
                    isGiftCard = true;
                    ePriceReason = PriceReason.Free;
                    priceObject = new Price()
                    {
                        m_dPrice = 0.0,
                        m_oCurrency = new Currency()
                        {
                            m_sCurrencyCD3 = currency
                        }
                    };
                }
                else
                {
                    priceObject = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(contentId, ppvModule, siteguid, couponCode, groupId,
                                                                                                  ref ePriceReason, ref relevantSub, ref relevantCol, ref relevantPP,
                                                                                                  string.Empty, string.Empty, deviceName);
                }
                bool couponFullDiscount = (ePriceReason == PriceReason.Free) && coupon != null;

                if (ePriceReason == PriceReason.ForPurchase ||
                    (ePriceReason == PriceReason.SubscriptionPurchased && priceObject.m_dPrice > 0) ||
                    couponFullDiscount ||
                    isGiftCard)
                {
                    // item is for purchase
                    if (priceObject.m_dPrice == price && priceObject.m_oCurrency.m_sCurrencyCD3 == currency)
                    {
                        string country = string.Empty;
                        if (!string.IsNullOrEmpty(userIp))
                        {
                            // get country by user IP
                            country = TVinciShared.WS_Utils.GetIP2CountryCode(userIp);
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
                            response = HandleFullCouponPurchase(cas, groupId, siteguid, price, currency, userIp, customData, productId, eTransactionType.PPV, billingGuid, contentId, isGiftCard);
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
                                // purchase passed
                                long purchaseId = 0;

                                // update entitlement date
                                DateTime entitlementDate = DateTime.UtcNow;
                                response.CreatedAt = DateUtils.DateTimeToUnixTimestamp(entitlementDate);

                                // grant entitlement
                                bool handleBillingPassed = cas.HandlePPVBillingSuccess(ref response, siteguid, householdId, relevantSub, price, currency, couponCode, userIp,
                                                                                   country, deviceName, long.Parse(response.TransactionID), customData, ppvModule,
                                                                                   productId, contentId, billingGuid, entitlementDate, ref purchaseId);

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
                    response.Status = Utils.SetResponseStatus(ePriceReason);
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

            string logString = string.Format("fail get response from billing service siteGUID={0}, houseHoldID={1}, price={2}, currency={3}, userIP={4}, customData={5}, productID={6}, (int)transactionType={7}, billingGuid={8}, paymentGWId={9}, paymentMethodId = {10}, adapterData = {11}",
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
                string userName = string.Empty;
                string password = string.Empty;
                module wsBillingService = null;
                Utils.InitializeBillingModule(ref wsBillingService, groupId, ref userName, ref password);

                // call new billing method for charge adapter
                var transactionResponse = wsBillingService.Transact(userName, password, siteGUID, (int)houseHoldID, price, currency, userIP, customData, productID, transactionType, contentId, billingGuid, paymentGWId, paymentMethodId, adapterData);

                response = BaseConditionalAccess.ConvertTransactResultToTransactionResponse(logString, transactionResponse);
            }
            catch (Exception ex)
            {
                log.Error(logString, ex);
                response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        internal static TransactionResponse HandleFullCouponPurchase(BaseConditionalAccess cas, int groupId, string siteGUID, double price, string currency, string userIP, string customData,
                                                  int productID, eTransactionType transactionType, string billingGuid, int contentId, bool isGiftCard = false)
        {
            TransactionResponse response = new TransactionResponse();

            try
            {
                string userName = string.Empty;
                string password = string.Empty;
                module wsBillingService = null;
                Utils.InitializeBillingModule(ref wsBillingService, groupId, ref userName, ref password);

                string extraParams = string.Empty;
                if (isGiftCard)
                {
                    extraParams = "GIFT_CARD";
                }

                // call new billing method for charge adapter
                var transactionResponse = wsBillingService.CC_DummyChargeUser(userName, password, siteGUID, price, currency, userIP, customData, 1, 1, extraParams);
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
